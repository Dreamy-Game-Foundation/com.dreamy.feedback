using System;
using UnityEngine;

namespace Dreamy.Feedback
{
    /// <summary>Owns playback services and their scene lifetime. Does not create gameplay or save services.</summary>
    public sealed class FeedbackHost : MonoBehaviour
    {
        [SerializeField] private FeedbackRoot root;
        [SerializeField] private VfxDatabase vfxDatabase;
        [SerializeField] private FloatingTextDatabase floatingTextDatabase;
        [SerializeField] private CameraShakeDatabase cameraShakeDatabase;
        [SerializeField] private FeedbackSequenceDatabase sequenceDatabase;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private bool registerServices;
        [SerializeField] private Sprite rewardIcon;
        private IDisposable registration;
        public VfxService Vfx { get; private set; }
        public HapticService Haptic { get; private set; }
        public FloatingTextService FloatingText { get; private set; }
        public IconFlyService IconFly { get; private set; }
        public ScreenFeedbackService Screen { get; private set; }
        public CameraShakeService CameraShake { get; private set; }
        public FeedbackSequenceService Sequence { get; private set; }
        public bool IsInitialized => Vfx != null;

        private void OnEnable() { if (root && vfxDatabase && floatingTextDatabase) Initialize(); }
        public void Initialize()
        {
            if (IsInitialized) return;
            if (!root || !vfxDatabase || !floatingTextDatabase || !worldCamera)
                throw new InvalidOperationException("FeedbackHost needs a root, VFX/text databases and an explicit world camera.");
            Vfx = new VfxService(); Vfx.Initialize(vfxDatabase, root.WorldVfxRoot);
            Haptic = new HapticService();
            FloatingText = new FloatingTextService(); FloatingText.Initialize(floatingTextDatabase, root.FloatingTextRoot, worldCamera);
            IconFly = new IconFlyService(); IconFly.Initialize(root.IconFlyRoot);
            Screen = new ScreenFeedbackService(); Screen.Initialize(root.ScreenFeedbackRoot);
            CameraShake = new CameraShakeService(); CameraShake.Initialize(cameraShakeDatabase, worldCamera);
            Sequence = new FeedbackSequenceService();
            Sequence.Initialize(sequenceDatabase, new FeedbackSequenceServices
            {
                Vfx = Vfx, Haptic = Haptic, FloatingText = FloatingText, IconFly = IconFly,
                ScreenFeedback = Screen, CameraShake = CameraShake
            });
            if (registerServices) registration = FeedbackServiceRegistry.RegisterOwned(Vfx, Haptic, FloatingText, IconFly, Screen, CameraShake, Sequence);
        }
        public void PlayReward(long amount, Vector3 worldPosition)
        {
            Initialize();
            Sequence.Play("reward", FeedbackContext.At(worldPosition));
            FloatingText.Play("+" + amount, worldPosition);
            var rect = root.IconFlyRoot as RectTransform;
            if (rewardIcon && rect)
            {
                var start = rect.TransformPoint(new Vector3(rect.rect.xMin + rect.rect.width * .3f, rect.rect.yMin + rect.rect.height * .68f, 0));
                var end = rect.TransformPoint(new Vector3(rect.rect.xMin + rect.rect.width * .85f, rect.rect.yMin + rect.rect.height * .9f, 0));
                var options = IconFlyOptions.Create(rewardIcon, start, end); options.Count = 8; IconFly.Fly(options);
            }
        }
        public void StopAll()
        {
            Sequence?.Stop(); CameraShake?.Stop(); Screen?.Stop();
            Vfx?.Clear(); FloatingText?.Clear(); IconFly?.Clear();
        }
        private void OnDisable() => Shutdown();
        private void OnDestroy() => Shutdown();
        public void Shutdown()
        {
            registration?.Dispose(); registration = null;
            Sequence?.Dispose(); CameraShake?.Dispose(); Screen?.Dispose();
            Vfx?.Dispose(); FloatingText?.Dispose(); IconFly?.Dispose();
            Sequence = null; CameraShake = null; Screen = null; Vfx = null; FloatingText = null; IconFly = null; Haptic = null;
        }
    }
}
