using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Dreamy.Feedback
{
    public sealed class FloatingTextService : IFeedbackAssetLookup, IFloatingTextService, IFloatingTextPresetService, IDisposable
    {
        private FloatingTextDatabase assetDatabase;
        public void SetDatabase(FloatingTextDatabase value) { if (value) value.Rebuild(); assetDatabase = value; }
        public FeedbackAsset FindAsset(string id) => assetDatabase && assetDatabase.TryGetAsset(id, out var asset) ? asset : null;

        private const string DefaultStyleId = "default";
        private readonly Dictionary<FloatingTextInstance, FloatingTextInstance> entryByInstance = new Dictionary<FloatingTextInstance, FloatingTextInstance>();
        private readonly Dictionary<FloatingTextInstance, Stack<FloatingTextInstance>> pools = new Dictionary<FloatingTextInstance, Stack<FloatingTextInstance>>();
        private readonly Dictionary<FloatingTextInstance, long> active = new Dictionary<FloatingTextInstance, long>();
        private readonly Dictionary<FloatingTextInstance, FeedbackPlayback> playback = new Dictionary<FloatingTextInstance, FeedbackPlayback>();
        private IFeedbackAnimationBackend backend = new UnityFeedbackAnimationBackend();
        public int MaximumActiveTexts { get; set; } = 64;
        private readonly HashSet<FloatingTextInstance> prewarmed = new HashSet<FloatingTextInstance>();
        private long generation;
        private Camera worldCamera;
        private FloatingTextDatabase database;
        private Transform root;

        public void Initialize(FloatingTextDatabase database, Transform root)
        {
            Initialize(database, root, null);
        }

        public void Initialize(FloatingTextDatabase database, Transform root, Camera worldCamera, IFeedbackAnimationBackend backend = null)
        {
            Clear();
            this.backend = backend ?? new UnityFeedbackAnimationBackend();
            this.worldCamera = worldCamera;
            this.database = database; SetDatabase(database);
            this.root = root;
            Prewarm();
        }

        public FeedbackHandle Play(string text, Vector3 worldPosition)
        {
            return Play(text, worldPosition, FloatingTextOptions.Style(DefaultStyleId));
        }

        public FeedbackHandle Play(string text, Vector3 worldPosition, FloatingTextOptions options)
            => PlayText(text, worldPosition, options, null);

        public FeedbackHandle PlayAtUi(string text, RectTransform anchor, FloatingTextOptions options)
            => PlayText(text, default, options, anchor);

        private FeedbackHandle PlayText(string text, Vector3 worldPosition, FloatingTextOptions options, RectTransform anchor)
        {
            if (active.Count >= MaximumActiveTexts) return default;
            if (!database)
            {
                Debug.LogWarning("FloatingTextService.Play called before Initialize.");
                return FeedbackHandle.Invalid;
            }

            var styleId = string.IsNullOrWhiteSpace(options.StyleId) ? DefaultStyleId : options.StyleId;
            if (FindAsset(styleId) is FloatingTextFeedbackAsset preset)
            {
                var direct = preset.Settings; direct.Color = options.Color ?? direct.Color; direct.Duration = options.Duration ?? direct.Duration;
                direct.MoveOffset = options.MoveOffset ?? direct.MoveOffset; direct.StartScale = options.StartScale ?? direct.StartScale; direct.EndScale = options.EndScale ?? direct.EndScale;
                return anchor ? PlayAtUi(direct, text, anchor, options.UnscaledTime) : Play(direct, text, worldPosition, options.UnscaledTime);
            }
            if (!database.TryGet(styleId, out var entry) || entry == null)
            {
                Debug.LogWarning($"Floating text style '{styleId}' was not found.");
                return FeedbackHandle.Invalid;
            }

            if (!entry.Prefab)
            {
                Debug.LogWarning($"Floating text style '{styleId}' has no prefab.");
                return FeedbackHandle.Invalid;
            }

            var settings = new FloatingTextFeedbackSettings { Prefab = entry.Prefab.gameObject, Color = options.Color ?? entry.Color,
                Duration = options.Duration ?? entry.Duration, MoveOffset = options.MoveOffset ?? entry.MoveOffset,
                StartScale = options.StartScale ?? entry.StartScale, EndScale = options.EndScale ?? entry.EndScale };
            return anchor ? PlayAtUi(settings, text, anchor, options.UnscaledTime) : Play(settings, text, worldPosition, options.UnscaledTime);
        }

        public FeedbackHandle Play(FloatingTextFeedbackSettings settings, string text, Vector3 worldPosition, bool unscaledTime)
            => PlayText(settings, text, worldPosition, unscaledTime, false);

        public FeedbackHandle PlayAtUi(FloatingTextFeedbackSettings settings, string text, RectTransform anchor, bool unscaledTime)
        {
            if (!FeedbackUtility.TryUiPosition(anchor, root, out var position)) return default;
            return PlayText(settings, text, position, unscaledTime, true);
        }

        private FeedbackHandle PlayText(FloatingTextFeedbackSettings settings, string text, Vector3 worldPosition, bool unscaledTime, bool uiPosition)
        {
            if (!root || active.Count >= MaximumActiveTexts || !settings.Prefab || !Finite(settings.Duration) || settings.Duration < 0 ||
                !Finite(settings.StartScale) || !Finite(settings.EndScale)) return default;
            var prefab = settings.Prefab.GetComponent<FloatingTextInstance>();
            if (!prefab) { Debug.LogWarning("Floating text prefab requires FloatingTextInstance on its root."); return default; }
            PrewarmPrefab(prefab, settings.PrewarmCount);
            var instance = Get(prefab);
            long lease = ++generation; active[instance] = lease;

            if (!uiPosition && worldCamera) worldPosition = FeedbackUtility.WorldToUIPosition(worldPosition, worldCamera, root);
            var state = new FeedbackPlayback(); playback[instance] = state;
            var animation = instance.Play(text, worldPosition, settings.Color, settings.Duration, settings.MoveOffset, settings.StartScale, settings.EndScale, unscaledTime, backend);
            state.Bind(() => { animation.Stop(); Release(instance); }, () => { animation.Complete(); Release(instance); });
            Observe(instance, lease, state, animation).Forget();
            return state.Handle;
        }

        private async UniTaskVoid Observe(FloatingTextInstance instance, long lease, FeedbackPlayback state, FeedbackHandle animation)
        {
            var status = await animation.Completion.AsUniTask();
            if (state.Status != FeedbackStatus.Running) return;
            if (status == FeedbackStatus.Completed) { Release(instance); state.Finish(); }
            else if (status == FeedbackStatus.Faulted) state.Fail();
            else state.Stop();
        }

        public void Prewarm()
        {
            if (!database)
            {
                return;
            }

            for (var i = 0; i < database.Entries.Count; i++)
            {
                var entry = database.Entries[i];
                if (entry == null || !entry.Prefab)
                {
                    continue;
                }

                PrewarmPrefab(entry.Prefab, entry.PrewarmCount);
            }
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        private void PrewarmPrefab(FloatingTextInstance prefab, int count)
        {
            if (!prewarmed.Add(prefab)) return;
            for (int i = 0; i < Mathf.Clamp(count, 0, 64); i++)
            { var instance = Create(prefab); active[instance] = ++generation; Release(instance); }
        }

        public void Clear()
        {
            foreach (var state in new List<FeedbackPlayback>(playback.Values)) state.Stop(); playback.Clear();
            active.Clear(); pools.Clear();
            foreach (var instance in entryByInstance.Keys)
            {
                if (!instance) continue;
                instance.gameObject.SetActive(false); UnityEngine.Object.Destroy(instance.gameObject);
            }
            entryByInstance.Clear(); prewarmed.Clear();
        }
        public void Dispose() { Clear(); database = null; root = null; worldCamera = null; }

        private FloatingTextInstance Get(FloatingTextInstance prefab)
        {
            if (pools.TryGetValue(prefab, out var pool))
            {
                while (pool.Count > 0)
                {
                    var pooled = pool.Pop();
                    if (pooled)
                    {
                        return pooled;
                    }
                }
            }

            return Create(prefab);
        }

        private FloatingTextInstance Create(FloatingTextInstance prefab)
        {
            var instance = UnityEngine.Object.Instantiate(prefab, root);
            entryByInstance[instance] = prefab;
            return instance;
        }

        private void Release(FloatingTextInstance instance)
        {
            if (!instance || !active.Remove(instance)) return;

            if (playback.TryGetValue(instance, out var state)) { playback.Remove(instance); state.Finish(); }
            instance.gameObject.SetActive(false);
            instance.transform.SetParent(root, false);
            if (!entryByInstance.TryGetValue(instance, out var prefab))
            {
                UnityEngine.Object.Destroy(instance.gameObject);
                return;
            }

            if (!pools.TryGetValue(prefab, out var pool))
            {
                pool = new Stack<FloatingTextInstance>();
                pools[prefab] = pool;
            }

            pool.Push(instance);
        }
    }
}
