using UnityEngine;
namespace Dreamy.Feedback
{
    public interface IFloatingTextPresetService
    {
        FeedbackHandle Play(FloatingTextFeedbackSettings settings, string text, Vector3 worldPosition, bool unscaledTime);
    }
}
