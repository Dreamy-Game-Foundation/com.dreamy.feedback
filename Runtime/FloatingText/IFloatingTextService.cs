using UnityEngine;

namespace Dreamy.Feedback
{
    public interface IFloatingTextService
    {
        void Initialize(FloatingTextDatabase database, Transform root);
        FeedbackHandle Play(string text, Vector3 worldPosition);
        FeedbackHandle Play(string text, Vector3 worldPosition, FloatingTextOptions options);
        void Prewarm();
        void Clear();
    }
}
