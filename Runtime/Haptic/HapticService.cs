using UnityEngine;

namespace Dreamy.Feedback
{
    public sealed class HapticService : IHapticService
    {
        public bool Enabled { get; set; } = true;
        public float MinimumInterval { get; set; } = .1f;
        private float lastPlayTime = float.NegativeInfinity;

        public void Play(HapticType type)
        {
            if (!Enabled || type == HapticType.None || Time.unscaledTime - lastPlayTime < MinimumInterval)
            {
                return;
            }

            // Unity's built-in mobile fallback has one vibration intensity.
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            lastPlayTime = Time.unscaledTime;
            Handheld.Vibrate();
#endif
        }
    }
}
