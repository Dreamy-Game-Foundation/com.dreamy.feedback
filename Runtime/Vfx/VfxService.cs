using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Dreamy.Feedback
{
    public sealed class VfxService : IVfxService, IDisposable
    {
        private readonly Dictionary<GameObject, Stack<VfxInstance>> pools = new Dictionary<GameObject, Stack<VfxInstance>>();
        private readonly Dictionary<VfxInstance, GameObject> prefabByInstance = new Dictionary<VfxInstance, GameObject>();
        private readonly Dictionary<VfxInstance, CancellationTokenSource> despawnTokens = new Dictionary<VfxInstance, CancellationTokenSource>();
        private readonly Dictionary<VfxInstance, long> active = new Dictionary<VfxInstance, long>();
        private readonly Dictionary<VfxInstance, FeedbackPlayback> playback = new Dictionary<VfxInstance, FeedbackPlayback>();
        public int MaximumActiveVfx { get; set; } = 64;
        private long generation;
        private VfxDatabase database;
        private Transform root;

        public void Initialize(VfxDatabase database, Transform root)
        {
            Clear();
            this.database = database;
            this.root = root;
            Prewarm();
        }

        public FeedbackHandle Play(string id, Vector3 worldPosition)
        {
            return Play(id, VfxPlayOptions.At(worldPosition));
        }

        public FeedbackHandle Play(string id, VfxPlayOptions options)
        {
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

            var instance = Get(entry.Prefab);
            var parent = options.Parent ? options.Parent : root;
            instance.transform.SetParent(parent, false);
            instance.transform.localScale = entry.Prefab.transform.localScale * (options.Scale == 0 ? 1 : options.Scale);
            instance.transform.rotation = options.Rotation == default ? Quaternion.identity : options.Rotation;
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
            instance.Play();
            if (options.FollowTarget)
            {
                instance.Follow(options.FollowTarget);
            }

            ScheduleDespawn(instance, entry, options.UnscaledTime);
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

                for (var j = 0; j < Mathf.Clamp(entry.PrewarmCount,0,64); j++)
                {
                    var instance = Create(entry.Prefab); active[instance] = ++generation; Release(instance);
                }
            }
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
            prefabByInstance.Clear();
        }
        public void Dispose() { Clear(); database = null; root = null; }

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

        private void ScheduleDespawn(VfxInstance instance, VfxEntry entry, bool unscaled)
        {
            if (entry.DespawnMode == VfxDespawnMode.Manual)
            {
                return;
            }

            var lifetime = entry.FixedLifetime;
            if (entry.DespawnMode == VfxDespawnMode.ParticleDuration && instance is ParticleVfxInstance particleVfx)
            {
                lifetime = particleVfx.GetDuration();
            }

            if (lifetime <= 0f)
            {
                Debug.LogWarning($"VFX id '{entry.Id}' has invalid lifetime.");
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
