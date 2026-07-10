namespace Dreamy.Feedback
{
    public sealed class FeedbackSequenceServices
    {
        public IVfxService Vfx { get; set; }
        public IHapticService Haptic { get; set; }
        public IFloatingTextService FloatingText { get; set; }
        public IIconFlyService IconFly { get; set; }
        public IScreenFeedbackService ScreenFeedback { get; set; }
        public ICameraShakeService CameraShake { get; set; }
    }
}
