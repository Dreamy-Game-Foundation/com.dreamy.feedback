namespace Dreamy.Feedback
{
    public interface IScreenFeedbackService
    {
        void Initialize(UnityEngine.Transform root);
        FeedbackHandle Flash(ScreenFlashOptions options);
        FeedbackHandle Fade(ScreenFlashOptions options);
    }
}
