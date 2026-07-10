namespace Dreamy.Feedback
{
    public interface IFeedbackSequenceService
    {
        void Initialize(FeedbackSequenceDatabase database, FeedbackSequenceServices services);
        FeedbackHandle Play(string id, FeedbackContext context);
    }
}
