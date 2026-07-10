using UnityEngine;

namespace Dreamy.Feedback.Samples
{
    public sealed class BasicFeedbackSample : MonoBehaviour
    {
        [SerializeField] private FeedbackRoot feedbackRoot;
        [SerializeField] private VfxDatabase vfxDatabase;
        [SerializeField] private FloatingTextDatabase floatingTextDatabase;
        [SerializeField] private CameraShakeDatabase cameraShakeDatabase;
        [SerializeField] private string vfxId = "level_up";
        [SerializeField] private string cameraShakeId = "small";

        private IVfxService vfx;
        private IHapticService haptic;
        private IFloatingTextService floatingText;
        private IScreenFeedbackService screenFeedback;
        private ICameraShakeService cameraShake;

        private void Awake()
        {
            if (!feedbackRoot)
            {
                feedbackRoot = FindFirstObjectByType<FeedbackRoot>();
            }

            vfx = new VfxService();
            vfx.Initialize(vfxDatabase, feedbackRoot.WorldVfxRoot);

            haptic = new HapticService();

            floatingText = new FloatingTextService();
            floatingText.Initialize(floatingTextDatabase, feedbackRoot.FloatingTextRoot);

            screenFeedback = new ScreenFeedbackService();
            screenFeedback.Initialize(feedbackRoot.ScreenFeedbackRoot);

            cameraShake = new CameraShakeService();
            cameraShake.Initialize(cameraShakeDatabase, Camera.main);

            FeedbackServiceRegistry.Register(vfx, haptic, floatingText, null, screenFeedback, cameraShake);
        }

        public void PlaySample()
        {
            var position = transform.position;
            vfx.Play(vfxId, position);
            haptic.Play(HapticType.Light);
            floatingText.Play("+100", position);
            screenFeedback.Flash(ScreenFlashOptions.WhiteFlash());
            cameraShake.Shake(cameraShakeId);
        }
    }
}
