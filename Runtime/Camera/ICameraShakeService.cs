using UnityEngine;

namespace Dreamy.Feedback
{
    public interface ICameraShakeService
    {
        void Initialize(CameraShakeDatabase database, Camera camera);
        FeedbackHandle Shake(string id);
        FeedbackHandle Shake(CameraShakeOptions options);
    }
}
