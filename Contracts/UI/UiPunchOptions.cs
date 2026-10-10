namespace Dreamy.Feedback
{
    [System.Serializable]
    public struct UiPunchOptions
    {
        public float Duration;
        public float Strength;
        public int Vibrato;
        public bool UnscaledTime;
        public static UiPunchOptions Default => new UiPunchOptions { Duration = .35f, Strength = .2f, Vibrato = 3, UnscaledTime = true };
    }
}
