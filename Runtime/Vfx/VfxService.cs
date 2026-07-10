using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Dreamy.Feedback
{
    public sealed class VfxService : IVfxService
    {
        private readonly Dictionary<GameObject, Stack<VfxInstance>> pools = new Dictionary<GameObject, Stack<VfxInstance>>();
        private readonly Dictionary<VfxInstance, GameObject> prefabByInstance = new Dictionary<VfxInstance, GameObject>();
        private readonly Dictionary<VfxInstance, CancellationTokenSource> despawnTokens = new Dictionary<VfxInstance, CancellationTokenSource>();
        private VfxDatabase database;
        private Transform root;

        public void Initialize(VfxDatabase database, Transform root)
        {
            this.database = database;
            this.root = root;
            Prewarm();
        }

        public VfxHandle Play(string id, Vector3 worldPosition)
        {
            return Play(id, VfxPlayOptions.At(worldPosition));
        }

        public VfxHandle Play(string id, VfxPlayOptions options)
        {
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
            instance.transform.rotation = options.Rotation == default ? Quaternion.identity : options.Rotation;
            if (options.UseLocalPosition)
            {
                instance.transform.localPosition = options.Position;
            }
            else
            {
                instance.transform.position = options.Position;
            }

            instance.Initialize(Release);
            instance.Play();
            if (options.FollowTarget)
            {
                instance.Follow(options.FollowTarget);
            }

            ScheduleDespawn(instance, entry);
            return new VfxHandle(instance);
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

                for (var j = 0; j < entry.PrewarmCount; j++)
                {
                    Release(Create(entry.Prefab));
                }
            }
        }

        public void Clear()
        {
            foreach (var pool in pools.Values)
            {
                while (pool.Count > 0)
                {
                    var instance = pool.Pop();
                    if (instance)
                    {
                        Object.Destroy(instance.gameObject);
                    }
                }
            }

            pools.Clear();
            prefabByInstance.Clear();
        }

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
            var go = Object.Instantiate(prefab, root);
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
            if (!instance)
            {
                return;
            }

            instance.gameObject.SetActive(false);
            CancelDespawn(instance);
            instance.transform.SetParent(root, false);
            if (!prefabByInstance.TryGetValue(instance, out var prefab) || !prefab)
            {
                Object.Destroy(instance.gameObject);
                return;
            }

            if (!pools.TryGetValue(prefab, out var pool))
            {
                pool = new Stack<VfxInstance>();
                pools[prefab] = pool;
            }

            pool.Push(instance);
        }

        private void ScheduleDespawn(VfxInstance instance, VfxEntry entry)
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
            DespawnAfterAsync(instance, lifetime, cancellation.Token).Forget();
        }

        private async UniTaskVoid DespawnAfterAsync(VfxInstance instance, float lifetime, CancellationToken cancellationToken)
        {
            var canceled = await UniTask.Delay(System.TimeSpan.FromSeconds(lifetime), cancellationToken: cancellationToken)
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
