using UnityEngine;

namespace Dreamy.Feedback
{
    public struct CameraShakeOptions
    {
        public float Duration;
        public float Amplitude;
        public float Frequency;

        public static CameraShakeOptions Small => new CameraShakeOptions
        {
            Duration = 0.2f,
            Amplitude = 0.08f,
            Frequency = 30f
        };
    }
}
