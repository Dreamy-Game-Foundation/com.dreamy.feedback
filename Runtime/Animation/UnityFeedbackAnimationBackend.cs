using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
namespace Dreamy.Feedback
{
    public sealed class UnityFeedbackAnimationBackend : IFeedbackAnimationBackend
    {
        public FeedbackHandle Animate(float duration, bool unscaledTime, FeedbackEase ease, Action<float> update)
        {
            if (float.IsNaN(duration) || float.IsInfinity(duration)) return default;
            var p = new FeedbackPlayback();
            p.Bind(null, () => update(1));
            Run(p, Mathf.Max(0, duration), unscaledTime, ease, update).Forget();
            return p.Handle;
        }
        private static async UniTaskVoid Run(FeedbackPlayback p, float duration, bool unscaled, FeedbackEase ease, Action<float> update)
        {
            try
            {
                float elapsed = 0;
                while (p.Status == FeedbackStatus.Running)
                {
                    float t = duration <= 0 ? 1 : Mathf.Clamp01(elapsed / duration);
                    update(Evaluate(t, ease));
                    if (t >= 1) { p.Finish(); return; }
                    await UniTask.Yield();
                    elapsed += unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
                }
            }
            catch (Exception e) { p.Fail(); Debug.LogException(e); }
        }
        public static float Evaluate(float t, FeedbackEase ease)
        {
            switch (ease)
            {
                case FeedbackEase.OutCubic: return 1 - Mathf.Pow(1 - t, 3);
                case FeedbackEase.InCubic: return t * t * t;
                case FeedbackEase.InOutSine: return (1 - Mathf.Cos(t * Mathf.PI)) * .5f;
                case FeedbackEase.OutBack: return 1 + 2.70158f * Mathf.Pow(t - 1, 3) + 1.70158f * Mathf.Pow(t - 1, 2);
                default: return t;
            }
        }
    }
}
