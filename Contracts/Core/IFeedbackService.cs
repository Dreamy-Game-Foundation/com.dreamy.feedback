namespace Dreamy.Feedback
{
    public interface IFeedbackService
    {
        FeedbackHandle Play(FeedbackRequest request);
        void StopAll();
    }
}
