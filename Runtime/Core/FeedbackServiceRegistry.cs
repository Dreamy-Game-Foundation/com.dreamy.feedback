using Dreamy.Core;

namespace Dreamy.Feedback
{
    public static class FeedbackServiceRegistry
    {
        public static void Register(
            IVfxService vfx = null,
            IHapticService haptic = null,
            IFloatingTextService floatingText = null,
            IIconFlyService iconFly = null,
            IScreenFeedbackService screenFeedback = null,
            ICameraShakeService cameraShake = null,
            IFeedbackSequenceService sequence = null)
        {
            RegisterIfPresent(vfx);
            RegisterIfPresent(haptic);
            RegisterIfPresent(floatingText);
            RegisterIfPresent(iconFly);
            RegisterIfPresent(screenFeedback);
            RegisterIfPresent(cameraShake);
            RegisterIfPresent(sequence);
        }

        private static void RegisterIfPresent<T>(T service) where T : class
        {
            if (service != null)
            {
                ServiceLocator.Register(service);
            }
        }
    }
}
