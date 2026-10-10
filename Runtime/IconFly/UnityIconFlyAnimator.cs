using UnityEngine;
namespace Dreamy.Feedback
{
    public sealed class UnityIconFlyAnimator : IIconFlyAnimator
    {
        private readonly IFeedbackAnimationBackend backend;
        public UnityIconFlyAnimator(IFeedbackAnimationBackend backend) { this.backend = backend; }
        public FeedbackHandle Animate(Transform icon, CanvasGroup opacity, IconFlyOptions options, Vector3 spread, int index)
        {
            float delay = Mathf.Max(0, options.Stagger) * index;
            float duration = Mathf.Max(.01f, options.Duration);
            return backend.Animate(delay + duration, options.UnscaledTime, FeedbackEase.Linear, t =>
            {
                if (!icon) return;
                float local = Mathf.Clamp01((t * (delay + duration) - delay) / duration);
                IconFlyMotion.Apply(icon, opacity, options, spread, local);
            });
        }
    }
}
