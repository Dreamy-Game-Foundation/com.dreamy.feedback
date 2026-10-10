using UnityEngine;

namespace Dreamy.Feedback
{
    [System.Serializable]
    public struct CameraShakeOptions
    {
        public float Duration;
        public bool UnscaledTime;
        public float Amplitude;
        public float Frequency;

        public static CameraShakeOptions Small => new CameraShakeOptions
        {
            Duration = 0.35f,
            UnscaledTime = true,
            Amplitude = 0.18f,
            Frequency = 30f
        };
    }
}
