using System;
namespace Dreamy.Feedback
{
    public enum FeedbackEase { Linear, OutCubic, InCubic, InOutSine, OutBack }
    public interface IFeedbackAnimationBackend
    {
        FeedbackHandle Animate(float duration, bool unscaledTime, FeedbackEase ease, Action<float> update);
    }
}
