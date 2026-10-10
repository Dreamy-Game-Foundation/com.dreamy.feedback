namespace Dreamy.Feedback
{
    public interface IScreenFeedbackService
    {
        FeedbackHandle Flash(ScreenFlashOptions options);
        FeedbackHandle Fade(ScreenFlashOptions options);
    }
}
