namespace Dreamy.Feedback
{
    /// <summary>Optional direct-asset capability; existing ID-only implementations remain compatible.</summary>
    public interface IVfxPresetService
    {
        FeedbackHandle Play(VfxFeedbackSettings settings, VfxPlayOptions options);
    }
}
