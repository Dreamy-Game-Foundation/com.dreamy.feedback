using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dreamy.Feedback
{
    public sealed class IconFlyService : IIconFlyService, IDisposable
    {
        private const string IconFlyName = "IconFlyInstance";
        private readonly Stack<IconFlyInstance> pool = new Stack<IconFlyInstance>();
        private readonly HashSet<IconFlyInstance> instances = new HashSet<IconFlyInstance>();
        private readonly HashSet<IconFlyInstance> active = new HashSet<IconFlyInstance>();
        private Transform root;

        public void Initialize(Transform root)
        {
            Clear();
            this.root = root;
        }

        public void Fly(IconFlyOptions options)
        {
            if (!root)
            {
                Debug.LogWarning("IconFlyService.Fly called before Initialize.");
                return;
            }

            var count = Mathf.Clamp(options.Count, 1, 64);
            for (var i = 0; i < count; i++)
            {
                var instance = Get();
                active.Add(instance);
                instance.Initialize(Release);
                var offset = new Vector3(UnityEngine.Random.Range(-options.Spread, options.Spread), UnityEngine.Random.Range(-options.Spread, options.Spread), 0f);
                instance.Fly(options, offset);
            }
        }

        public void Clear()
        {
            active.Clear(); pool.Clear();
            foreach (var instance in instances) if (instance) { instance.gameObject.SetActive(false); UnityEngine.Object.Destroy(instance.gameObject); }
            instances.Clear();
        }
        public void Dispose() { Clear(); root = null; }

        private IconFlyInstance Get()
        {
            while (pool.Count > 0)
            {
                var pooled = pool.Pop();
                if (pooled)
                {
                    return pooled;
                }
            }

            var go = new GameObject(IconFlyName, typeof(RectTransform));
            go.transform.SetParent(root, false);
            var created = go.AddComponent<IconFlyInstance>(); instances.Add(created); return created;
        }

        private void Release(IconFlyInstance instance)
        {
            if (!instance || !active.Remove(instance)) return;

            instance.gameObject.SetActive(false);
            instance.transform.SetParent(root, false);
            pool.Push(instance);
        }
    }
}
