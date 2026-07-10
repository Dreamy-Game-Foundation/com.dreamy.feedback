using System.Collections.Generic;
using UnityEngine;

namespace Dreamy.Feedback
{
    public sealed class FloatingTextService : IFloatingTextService
    {
        private const string DefaultStyleId = "default";
        private readonly Dictionary<FloatingTextInstance, FloatingTextEntry> entryByInstance = new Dictionary<FloatingTextInstance, FloatingTextEntry>();
        private readonly Dictionary<FloatingTextEntry, Stack<FloatingTextInstance>> pools = new Dictionary<FloatingTextEntry, Stack<FloatingTextInstance>>();
        private FloatingTextDatabase database;
        private Transform root;

        public void Initialize(FloatingTextDatabase database, Transform root)
        {
            this.database = database;
            this.root = root;
            Prewarm();
        }

        public FeedbackHandle Play(string text, Vector3 worldPosition)
        {
            return Play(text, worldPosition, FloatingTextOptions.Style(DefaultStyleId));
        }

        public FeedbackHandle Play(string text, Vector3 worldPosition, FloatingTextOptions options)
        {
            if (!database)
            {
                Debug.LogWarning("FloatingTextService.Play called before Initialize.");
                return FeedbackHandle.Invalid;
            }

            var styleId = string.IsNullOrWhiteSpace(options.StyleId) ? DefaultStyleId : options.StyleId;
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

            var instance = Get(entry);
            var color = options.Color ?? entry.Color;
            var duration = options.Duration ?? entry.Duration;
            var moveOffset = options.MoveOffset ?? entry.MoveOffset;
            var startScale = options.StartScale ?? entry.StartScale;
            var endScale = options.EndScale ?? entry.EndScale;
            instance.Initialize(Release);
            instance.Play(text, worldPosition, color, duration, moveOffset, startScale, endScale);
            return new FeedbackHandle(true, () => Release(instance));
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
                    Release(Create(entry));
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
            entryByInstance.Clear();
        }

        private FloatingTextInstance Get(FloatingTextEntry entry)
        {
            if (pools.TryGetValue(entry, out var pool))
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

            return Create(entry);
        }

        private FloatingTextInstance Create(FloatingTextEntry entry)
        {
            var instance = Object.Instantiate(entry.Prefab, root);
            entryByInstance[instance] = entry;
            return instance;
        }

        private void Release(FloatingTextInstance instance)
        {
            if (!instance)
            {
                return;
            }

            instance.gameObject.SetActive(false);
            instance.transform.SetParent(root, false);
            if (!entryByInstance.TryGetValue(instance, out var entry))
            {
                Object.Destroy(instance.gameObject);
                return;
            }

            if (!pools.TryGetValue(entry, out var pool))
            {
                pool = new Stack<FloatingTextInstance>();
                pools[entry] = pool;
            }

            pool.Push(instance);
        }
    }
}
