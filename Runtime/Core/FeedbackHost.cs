using System;
using UnityEngine;
namespace Dreamy.Feedback
{
    public sealed class FeedbackHost : MonoBehaviour
    {
        [SerializeField] private FeedbackRoot root;
        [SerializeField] private VfxDatabase vfxDatabase;
        [SerializeField] private FloatingTextDatabase floatingTextDatabase;
        [SerializeField] private CameraShakeDatabase cameraShakeDatabase;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private FeedbackAnimationProvider animationProvider;
        [SerializeField] private bool haptics = true;
        [SerializeField] private bool iconFly = true;
        [SerializeField] private bool screenFlash = true;
        public VfxService Vfx { get; private set; }
        public HapticService Haptic { get; private set; }
        public FloatingTextService FloatingText { get; private set; }
        public IconFlyService IconFly { get; private set; }
        public ScreenFeedbackService Screen { get; private set; }
        public CameraShakeService CameraShake { get; private set; }
        public UiFeedbackService Ui { get; private set; }
        public FeedbackService Feedback { get; private set; }
        public bool IsInitialized => Feedback != null;
        private void OnEnable() { if (root) Initialize(); }
        /// <summary>Bind a game camera. Changing cameras stops current playback and rebuilds the services.</summary>
        public void Initialize(Camera camera)
        {
            if (worldCamera != camera) { Shutdown(); worldCamera = camera; }
            Initialize();
        }
        public void Initialize()
        {
            if (IsInitialized) return;
            if (!root) throw new InvalidOperationException("FeedbackHost needs an explicit FeedbackRoot.");
            var backend = animationProvider ? animationProvider.CreateBackend() : new UnityFeedbackAnimationBackend();
            try
            {
                if (vfxDatabase) { Vfx = new VfxService(); Vfx.Initialize(vfxDatabase,root.WorldVfxRoot); }
                if (haptics) Haptic = new HapticService();
                if (floatingTextDatabase) { FloatingText = new FloatingTextService(); FloatingText.Initialize(floatingTextDatabase,root.FloatingTextRoot,worldCamera,backend); }
                if (iconFly) { IconFly = new IconFlyService(); IconFly.Initialize(root.IconFlyRoot,animationProvider ? animationProvider.CreateIconAnimator(backend) : new UnityIconFlyAnimator(backend)); }
                if (screenFlash) { Screen = new ScreenFeedbackService(); Screen.Initialize(root.ScreenFeedbackRoot,backend); }
                if (worldCamera) { CameraShake = new CameraShakeService(); CameraShake.Initialize(cameraShakeDatabase,worldCamera,backend,worldCamera.transform.IsChildOf(root.CameraRoot) ? root.CameraRoot : worldCamera.transform); }
                Ui = new UiFeedbackService(backend);
                Feedback = new FeedbackService(new FeedbackServices { Vfx = Vfx, Haptic = Haptic, FloatingText = FloatingText, IconFly = IconFly, Screen = Screen, CameraShake = CameraShake, Ui = Ui },backend);
            }
            catch { Shutdown(); throw; }
        }
        public void StopAll()
        { Feedback?.StopAll(); Ui?.Dispose(); CameraShake?.Stop(); Screen?.Stop(); Vfx?.Clear(); FloatingText?.Clear(); IconFly?.Clear(); }
        private void OnDisable() => Shutdown();
        private void OnDestroy() => Shutdown();
        public void Shutdown()
        {
            Feedback?.Dispose(); Ui?.Dispose(); CameraShake?.Dispose(); Screen?.Dispose(); Vfx?.Dispose(); FloatingText?.Dispose(); IconFly?.Dispose();
            Feedback = null; Ui = null; CameraShake = null; Screen = null; Vfx = null; FloatingText = null; IconFly = null; Haptic = null;
        }
    }
}
