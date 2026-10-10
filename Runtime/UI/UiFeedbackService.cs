using System;
using System.Collections.Generic;
using UnityEngine;
namespace Dreamy.Feedback
{
    public sealed class UiFeedbackService : IUiFeedbackService, IDisposable
    {
        private readonly Dictionary<Transform, FeedbackHandle> active = new Dictionary<Transform, FeedbackHandle>();
        private readonly IFeedbackAnimationBackend backend;
        public UiFeedbackService(IFeedbackAnimationBackend backend) { this.backend = backend; }
        public FeedbackHandle Punch(Transform target, UiPunchOptions options)
        {
            if (!target) return FeedbackHandle.Invalid;
            if (active.TryGetValue(target, out var old)) old.Stop();
            var basis = target.localScale;
            var state = new FeedbackPlayback();
            FeedbackHandle animation = default;
            Action restore = () => { animation.Stop(); if (target) target.localScale = basis; active.Remove(target); };
            state.Bind(restore, restore);
            active[target] = state.Handle;
            animation = backend.Animate(Mathf.Max(.01f, options.Duration), options.UnscaledTime, FeedbackEase.Linear, t =>
            {
                if (!target || !target.gameObject.activeInHierarchy) { state.Stop(); return; }
                target.localScale = basis * (1 + Mathf.Sin(t * Mathf.PI * Mathf.Max(1, options.Vibrato)) * (1 - t) * options.Strength);
                if (t >= 1) { restore(); state.Finish(); }
            });
            return state.Handle;
        }
        public void Dispose() { foreach (var h in new List<FeedbackHandle>(active.Values)) h.Stop(); active.Clear(); }
    }
}
