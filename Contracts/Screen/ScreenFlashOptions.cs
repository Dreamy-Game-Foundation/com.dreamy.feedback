using UnityEngine;

namespace Dreamy.Feedback
{
    [System.Serializable]
    public struct ScreenFlashOptions
    {
        public Color Color;
        public bool UnscaledTime;
        public float FadeInDuration;
        public float HoldDuration;
        public float FadeOutDuration;
        public float MaxAlpha;

        public static ScreenFlashOptions WhiteFlash(float duration = 0.2f)
        {
            return new ScreenFlashOptions
            {
                Color = Color.white,
                FadeInDuration = duration * 0.25f,
                HoldDuration = 0f,
                FadeOutDuration = duration * 0.75f,
                MaxAlpha = 0.65f
            };
        }
    }
}
