using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Dreamy.Feedback
{
    public sealed class VfxService : IFeedbackAssetLookup, IVfxService, IVfxPresetService, IDisposable
    {
        private VfxDatabase assetDatabase;
        public void SetDatabase(VfxDatabase value) { if (value) value.Rebuild(); assetDatabase = value; }
        public FeedbackAsset FindAsset(string id) => assetDatabase && assetDatabase.TryGetAsset(id, out var asset) ? asset : null;

        private readonly Dictionary<GameObject, Stack<VfxInstance>> pools = new Dictionary<GameObject, Stack<VfxInstance>>();
        private readonly Dictionary<VfxInstance, GameObject> prefabByInstance = new Dictionary<VfxInstance, GameObject>();
        private readonly Dictionary<VfxInstance, CancellationTokenSource> despawnTokens = new Dictionary<VfxInstance, CancellationTokenSource>();
        private readonly Dictionary<VfxInstance, long> active = new Dictionary<VfxInstance, long>();
        private readonly Dictionary<VfxInstance, FeedbackPlayback> playback = new Dictionary<VfxInstance, FeedbackPlayback>();
        public int MaximumActiveVfx { get; set; } = 64;
        private readonly HashSet<GameObject> prewarmed = new HashSet<GameObject>();
        private long generation;
        private VfxDatabase database;
        private Transform root;
        private RectTransform uiRoot;

        public void Initialize(VfxDatabase database, Transform root, RectTransform uiRoot = null)
        {
            Clear();
            this.database = database; SetDatabase(database);
            this.root = root;
            this.uiRoot = uiRoot;
            Prewarm();
        }

        public FeedbackHandle Play(string id, Vector3 worldPosition)
        {
            return Play(id, VfxPlayOptions.At(worldPosition));
        }

        public FeedbackHandle Play(string id, VfxPlayOptions options)
        {
            if (FindAsset(id) is VfxFeedbackAsset preset) return this.Play(preset, options);
            if (active.Count >= MaximumActiveVfx) return default;
            if (!database)
            {
                Debug.LogWarning("VfxService.Play called before Initialize.");
                return default;
            }

            if (!database.TryGet(id, out var entry) || entry == null)
            {
                Debug.LogWarning($"VFX id '{id}' was not found.");
                return default;
            }

            if (!entry.Prefab)
            {
                Debug.LogWarning($"VFX id '{id}' has no prefab.");
                return default;
            }

            return PlayPrefab(entry.Prefab, options, entry.DespawnMode, entry.FixedLifetime);
        }

        public FeedbackHandle Play(VfxFeedbackSettings settings, VfxPlayOptions options)
        {
            if (!root || !settings.Prefab || active.Count >= MaximumActiveVfx ||
                !Finite(settings.MaximumDuration) || settings.MaximumDuration <= 0 || !Finite(settings.Scale) || settings.Scale < 0) return default;
            if (settings.Scale == 0) return FeedbackHandle.Completed;
            if (settings.Space == VfxSpace.UI)
            {
                var parent = options.Parent ? options.Parent : uiRoot;
                if (!(parent is RectTransform) || !parent.GetComponentInParent<Canvas>() || !settings.Prefab.GetComponent<RectTransform>())
                {
                    Debug.LogWarning("UI VFX requires a RectTransform prefab and a parent under a Canvas.");
                    return default;
                }
                options.Parent = parent;
                if (options.UiAnchor)
                {
                    if (!FeedbackUtility.TryUiPosition(options.UiAnchor, parent, out options.Position)) return default;
                    options.UseLocalPosition = false;
                }
            }
            PrewarmPrefab(settings.Prefab, settings.PrewarmCount);
            options.Scale = (options.Scale == 0 ? 1 : options.Scale) * settings.Scale;
            return PlayPrefab(settings.Prefab, options, VfxDespawnMode.FixedLifetime, settings.MaximumDuration, settings.Space == VfxSpace.UI);
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        private FeedbackHandle PlayPrefab(GameObject prefab, VfxPlayOptions options, VfxDespawnMode mode, float lifetime, bool uiSpace = false)
        {
            if (!root || !Finite(options.Scale)) return default;
            var instance = Get(prefab);
            var parent = options.Parent ? options.Parent : root;
            instance.transform.SetParent(parent, false);
            instance.transform.localScale = prefab.transform.localScale * (options.Scale == 0 ? 1 : options.Scale);
            var rotation = options.Rotation == default ? Quaternion.identity : options.Rotation;
            if (uiSpace) instance.transform.localRotation = rotation;
            else instance.transform.rotation = rotation;
            if (options.UseLocalPosition)
            {
                instance.transform.localPosition = options.Position;
            }
            else
            {
                instance.transform.position = options.Position;
            }

            long lease = ++generation;
            active[instance] = lease;
            var state = new FeedbackPlayback(); playback[instance] = state;
            instance.Initialize(Release, () => state.Stop());
            state.Bind(() => { if (active.TryGetValue(instance, out long current) && current == lease) instance.Stop(); },
                () => { if (active.TryGetValue(instance, out long current) && current == lease) instance.Stop(); });
            instance.ConfigurePlayback(options, lifetime);
            instance.Play();
            if (options.FollowTarget)
            {
                instance.Follow(options.FollowTarget);
            }

            ScheduleDespawn(instance, mode, lifetime, options.UnscaledTime);
            return state.Handle;
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

        private void PrewarmPrefab(GameObject prefab, int count)
        {
            if (!prewarmed.Add(prefab)) return;
            for (int i = 0; i < Mathf.Clamp(count, 0, 64); i++)
            { var instance = Create(prefab); active[instance] = ++generation; Release(instance); }
        }

        public void Clear()
        {
            foreach (var state in new List<FeedbackPlayback>(playback.Values)) state.Stop(); playback.Clear();
            foreach (var cancellation in despawnTokens.Values) { cancellation.Cancel(); cancellation.Dispose(); }
            despawnTokens.Clear(); active.Clear(); pools.Clear();
            foreach (var instance in prefabByInstance.Keys)
            {
                if (!instance) continue;
                instance.gameObject.SetActive(false);
                UnityEngine.Object.Destroy(instance.gameObject);
            }
            prefabByInstance.Clear(); prewarmed.Clear();
        }
        public void Dispose() { Clear(); database = null; root = null; uiRoot = null; }

        private VfxInstance Get(GameObject prefab)
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

        private VfxInstance Create(GameObject prefab)
        {
            var go = UnityEngine.Object.Instantiate(prefab, root);
            var instance = go.GetComponent<VfxInstance>();
            if (!instance)
            {
                instance = go.GetComponentInChildren<ParticleSystem>(true)
                    ? go.AddComponent<ParticleVfxInstance>()
                    : go.AddComponent<VfxInstance>();
            }

            prefabByInstance[instance] = prefab;
            return instance;
        }

        private void Release(VfxInstance instance)
        {
            if (!instance || !active.Remove(instance)) return;

            if (playback.TryGetValue(instance, out var state)) { playback.Remove(instance); state.Finish(); }
            instance.gameObject.SetActive(false);
            CancelDespawn(instance);
            instance.transform.SetParent(root, false);
            if (!prefabByInstance.TryGetValue(instance, out var prefab) || !prefab)
            {
                UnityEngine.Object.Destroy(instance.gameObject);
                return;
            }

            if (!pools.TryGetValue(prefab, out var pool))
            {
                pool = new Stack<VfxInstance>();
                pools[prefab] = pool;
            }

            pool.Push(instance);
        }

        private void ScheduleDespawn(VfxInstance instance, VfxDespawnMode mode, float lifetime, bool unscaled)
        {
            if (mode == VfxDespawnMode.Manual)
            {
                return;
            }

            if (mode == VfxDespawnMode.ParticleDuration && instance is ParticleVfxInstance particleVfx)
            {
                lifetime = particleVfx.GetDuration();
            }

            if (lifetime <= 0f)
            {
                Debug.LogWarning("VFX has invalid lifetime.");
                return;
            }

            CancelDespawn(instance);
            var cancellation = new CancellationTokenSource();
            despawnTokens[instance] = cancellation;
            DespawnAfterAsync(instance, lifetime, cancellation.Token, unscaled).Forget();
        }

        private async UniTaskVoid DespawnAfterAsync(VfxInstance instance, float lifetime, CancellationToken cancellationToken, bool unscaled)
        {
            var canceled = await UniTask.Delay(System.TimeSpan.FromSeconds(lifetime), ignoreTimeScale: unscaled, cancellationToken: cancellationToken)
                .SuppressCancellationThrow();
            if (!canceled && instance && instance.gameObject.activeSelf)
            {
                instance.Stop();
            }
        }

        private void CancelDespawn(VfxInstance instance)
        {
            if (!instance || !despawnTokens.TryGetValue(instance, out var cancellation))
            {
                return;
            }

            cancellation.Cancel();
            cancellation.Dispose();
            despawnTokens.Remove(instance);
        }
    }
}
