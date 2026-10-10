namespace Dreamy.Feedback
{
    public sealed class FeedbackServices
    {
        public IVfxService Vfx { get; set; }
        public IHapticService Haptic { get; set; }
        public IFloatingTextService FloatingText { get; set; }
        public IIconFlyService IconFly { get; set; }
        public IUiFeedbackService Ui { get; set; }
        public IScreenFeedbackService Screen { get; set; }
        public ICameraShakeService CameraShake { get; set; }
    }
}
