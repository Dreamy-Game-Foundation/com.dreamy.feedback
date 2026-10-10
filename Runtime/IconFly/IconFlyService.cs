using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
namespace Dreamy.Feedback
{
    public sealed class IconFlyService : IIconFlyService, IDisposable
    {
        private readonly Stack<IconFlyInstance> pool = new Stack<IconFlyInstance>();
        private readonly HashSet<IconFlyInstance> instances = new HashSet<IconFlyInstance>();
        private readonly HashSet<FeedbackHandle> groups = new HashSet<FeedbackHandle>();
        private IIconFlyAnimator animator;
        private Transform root;
        public int MaximumActiveIcons { get; set; } = 128;
        private int activeCount;
        public void Initialize(Transform root, IIconFlyAnimator animator = null)
        { Clear(); this.root = root; this.animator = animator ?? new UnityIconFlyAnimator(new UnityFeedbackAnimationBackend()); }
        public FeedbackHandle Fly(IconFlyOptions options)
        {
            if (!root || float.IsNaN(options.Duration) || float.IsInfinity(options.Duration) || options.Duration < 0 || float.IsNaN(options.Stagger) || float.IsInfinity(options.Stagger)) return default;
            int count = Mathf.Clamp(options.Count, 1, 64);
            if (activeCount + count > MaximumActiveIcons) return default;
            var state = new FeedbackPlayback(); var leases = new List<IconFlyInstance>(); var animations = new List<FeedbackHandle>();
            bool released = false;
            Action release = () =>
            {
                if (released) return; released = true;
                foreach (var lease in leases)
                {
                    activeCount--;
                    if (!lease) continue;
                    lease.gameObject.SetActive(false); lease.transform.SetParent(root, false);
                    lease.transform.localScale = Vector3.one; lease.transform.localRotation = Quaternion.identity;
                    pool.Push(lease);
                }
                groups.Remove(state.Handle);
            };
            state.Bind(() => { foreach (var h in animations) h.Stop(); release(); },
                () => { foreach (var h in animations) h.Complete(); release(); });
            groups.Add(state.Handle);
            try
            {
                for (int i = 0; i < count; i++)
                {
                    var instance = Get(); leases.Add(instance); activeCount++; instance.Prepare(options);
                    var offset = UnityEngine.Random.insideUnitCircle * Mathf.Max(0, options.Spread);
                    animations.Add(animator.Animate(instance.transform, instance.Opacity, options, new Vector3(offset.x,offset.y,0), i));
                }
                Observe(state, animations, leases, options.Target, release).Forget();
            }
            catch { state.Fail(); throw; }
            return state.Handle;
        }
        private static async UniTaskVoid Observe(FeedbackPlayback state, List<FeedbackHandle> animations, List<IconFlyInstance> leases, Transform target, Action release)
        {
            bool follow = target;
            while (state.Status == FeedbackStatus.Running)
            {
                if (follow && (!target || !target.gameObject.activeInHierarchy) || leases.Exists(i => !i || !i.gameObject.activeInHierarchy)) { state.Stop(); return; }
                if (animations.Exists(h => h.Status == FeedbackStatus.Faulted)) { state.Fail(); return; }
                if (animations.Exists(h => h.Status == FeedbackStatus.Stopped)) { state.Stop(); return; }
                if (animations.TrueForAll(h => !h.IsRunning)) { release(); state.Finish(); return; }
                await UniTask.Yield();
            }
        }
        public void Clear()
        {
            foreach (var group in new List<FeedbackHandle>(groups)) group.Stop(); groups.Clear();
            foreach (var i in instances) if (i) UnityEngine.Object.Destroy(i.gameObject);
            instances.Clear(); pool.Clear(); activeCount = 0;
        }
        public void Dispose() { Clear(); root = null; }
        private IconFlyInstance Get()
        {
            while (pool.Count > 0) { var pooled = pool.Pop(); if (pooled) return pooled; }
            var go = new GameObject("IconFlyInstance", typeof(RectTransform)); go.transform.SetParent(root, false);
            var instance = go.AddComponent<IconFlyInstance>(); instances.Add(instance); return instance;
        }
    }
}
