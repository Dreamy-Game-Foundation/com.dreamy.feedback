namespace Dreamy.Feedback
{
    public interface IIconFlyService
    {
        FeedbackHandle Fly(IconFlyOptions options);
        void Clear();
    }
}
