using UnityEngine;

namespace Dreamy.Feedback
{
    public interface IFloatingTextService
    {
        FeedbackHandle Play(string text, Vector3 worldPosition);
        FeedbackHandle Play(string text, Vector3 worldPosition, FloatingTextOptions options);
        void Prewarm();
        void Clear();
    }
}
