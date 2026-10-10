using UnityEngine;

namespace Dreamy.Feedback
{
    public interface ICameraShakeService
    {
        FeedbackHandle Shake(string id);
        FeedbackHandle Shake(CameraShakeOptions options);
    }
}
