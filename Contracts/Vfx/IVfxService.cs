using UnityEngine;

namespace Dreamy.Feedback
{
    public interface IVfxService
    {
        FeedbackHandle Play(string id, Vector3 worldPosition);
        FeedbackHandle Play(string id, VfxPlayOptions options);
        void Prewarm();
        void Clear();
    }
}
