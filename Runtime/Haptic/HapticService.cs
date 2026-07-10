using UnityEngine;

namespace Dreamy.Feedback
{
    public sealed class HapticService : IHapticService
    {
        public bool Enabled { get; set; } = true;

        public void Play(HapticType type)
        {
            if (!Enabled || type == HapticType.None)
            {
                return;
            }

            Handheld.Vibrate();
        }
    }
}
