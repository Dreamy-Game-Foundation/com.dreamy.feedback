using System;
using UnityEngine;
namespace Dreamy.Feedback
{
    public sealed class CameraShakeService : ICameraShakeService, IDisposable
    {
        private CameraShakeDatabase database;
        private Transform target;
        private IFeedbackAnimationBackend backend;
        private FeedbackHandle active;
        public void Initialize(CameraShakeDatabase database, Camera camera, IFeedbackAnimationBackend backend = null, Transform offsetRoot = null)
        { Stop(); this.database = database; target = offsetRoot ? offsetRoot : camera ? camera.transform : null; this.backend = backend ?? new UnityFeedbackAnimationBackend(); }
        public FeedbackHandle Shake(string id) => database && database.TryGet(id, out var preset) ? Shake(preset.Options) : default;
        public FeedbackHandle Shake(CameraShakeOptions options)
        {
            if (!target) return default;
            Stop(); var basis = target.localPosition; var state = new FeedbackPlayback(); FeedbackHandle animation = default;
            Action restore = () => { animation.Stop(); if (target) target.localPosition = basis; };
            state.Bind(restore, restore); active = state.Handle;
            animation = backend.Animate(Mathf.Max(.01f,options.Duration), options.UnscaledTime, FeedbackEase.Linear, t =>
            {
                if (!target || !target.gameObject.activeInHierarchy) { state.Stop(); return; }
                float phase = t * Mathf.Max(1,options.Frequency) * Mathf.Max(.01f,options.Duration) * Mathf.PI * 2;
                target.localPosition = basis + new Vector3(Mathf.Sin(phase), Mathf.Cos(phase * 1.37f), 0) * options.Amplitude * (1-t);
                if (t >= 1) { restore(); state.Finish(); }
            });
            return state.Handle;
        }
        public void Stop() => active.Stop();
        public void Dispose() { Stop(); target = null; database = null; }
    }
}
