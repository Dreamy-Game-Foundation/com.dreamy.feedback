using System;
using System.Collections.Generic;
using Dreamy.Core;

namespace Dreamy.Feedback
{
    public static class FeedbackServiceRegistry
    {
        public static IDisposable RegisterOwned(
            IVfxService vfx = null, IHapticService haptic = null, IFloatingTextService floatingText = null,
            IIconFlyService iconFly = null, IScreenFeedbackService screenFeedback = null,
            ICameraShakeService cameraShake = null, IFeedbackSequenceService sequence = null)
        {
            var registration = new Registration();
            registration.Add(vfx); registration.Add(haptic); registration.Add(floatingText);
            registration.Add(iconFly); registration.Add(screenFeedback); registration.Add(cameraShake); registration.Add(sequence);
            return registration;
        }
        private sealed class Registration : IDisposable
        {
            private readonly List<Action> cleanup = new List<Action>();
            public void Add<T>(T service) where T : class
            {
                if (service == null) return;
                ServiceLocator.Register(service);
                cleanup.Add(() => { if (ServiceLocator.TryGet<T>(out var current) && ReferenceEquals(current, service)) ServiceLocator.Unregister<T>(); });
            }
            public void Dispose() { foreach (var action in cleanup) action(); cleanup.Clear(); }
        }

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
