using UnityEngine;

namespace Dreamy.Feedback
{
    public sealed class FeedbackRoot : MonoBehaviour
    {
        private const string WorldVfxRootName = "WorldVfxRoot";
        private const string FloatingTextRootName = "FloatingTextRoot";
        private const string IconFlyRootName = "IconFlyRoot";
        private const string ScreenFeedbackRootName = "ScreenFeedbackRoot";
        private const string CameraRootName = "CameraRoot";

        [SerializeField] private Transform worldVfxRoot;
        [SerializeField] private Transform floatingTextRoot;
        [SerializeField] private Transform iconFlyRoot;
        [SerializeField] private Transform screenFeedbackRoot;
        [SerializeField] private Transform cameraRoot;

        public Transform WorldVfxRoot => worldVfxRoot ? worldVfxRoot : (worldVfxRoot = EnsureChild(WorldVfxRootName));
        public Transform FloatingTextRoot => floatingTextRoot ? floatingTextRoot : (floatingTextRoot = EnsureChild(FloatingTextRootName));
        public Transform IconFlyRoot => iconFlyRoot ? iconFlyRoot : (iconFlyRoot = EnsureChild(IconFlyRootName));
        public Transform ScreenFeedbackRoot => screenFeedbackRoot ? screenFeedbackRoot : (screenFeedbackRoot = EnsureChild(ScreenFeedbackRootName));
        public Transform CameraRoot => cameraRoot ? cameraRoot : (cameraRoot = EnsureChild(CameraRootName));

        private void Reset()
        {
            worldVfxRoot = EnsureChild(WorldVfxRootName);
            floatingTextRoot = EnsureChild(FloatingTextRootName);
            iconFlyRoot = EnsureChild(IconFlyRootName);
            screenFeedbackRoot = EnsureChild(ScreenFeedbackRootName);
            cameraRoot = EnsureChild(CameraRootName);
        }

        private Transform EnsureChild(string childName)
        {
            var child = transform.Find(childName);
            if (child)
            {
                return child;
            }

            var go = new GameObject(childName);
            go.transform.SetParent(transform, false);
            return go.transform;
        }
    }
}
