using System.Collections.Generic;
using UnityEngine;

namespace Dreamy.Feedback
{
    public sealed class IconFlyService : IIconFlyService
    {
        private const string IconFlyName = "IconFlyInstance";
        private readonly Stack<IconFlyInstance> pool = new Stack<IconFlyInstance>();
        private Transform root;

        public void Initialize(Transform root)
        {
            this.root = root;
        }

        public void Fly(IconFlyOptions options)
        {
            if (!root)
            {
                Debug.LogWarning("IconFlyService.Fly called before Initialize.");
                return;
            }

            var count = Mathf.Max(1, options.Count);
            for (var i = 0; i < count; i++)
            {
                var instance = Get();
                instance.Initialize(Release);
                var offset = new Vector3(Random.Range(-options.Spread, options.Spread), Random.Range(-options.Spread, options.Spread), 0f);
                instance.Fly(options, offset);
            }
        }

        public void Clear()
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
            return go.AddComponent<IconFlyInstance>();
        }

        private void Release(IconFlyInstance instance)
        {
            if (!instance)
            {
                return;
            }

            instance.gameObject.SetActive(false);
            instance.transform.SetParent(root, false);
            pool.Push(instance);
        }
    }
}
