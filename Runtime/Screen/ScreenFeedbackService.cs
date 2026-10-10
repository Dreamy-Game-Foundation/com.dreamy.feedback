using UnityEngine;

namespace Dreamy.Feedback
{
    public sealed class ScreenFeedbackService : IScreenFeedbackService, System.IDisposable
    {
        private ScreenFlashView view;

        public void Initialize(Transform root, IFeedbackAnimationBackend backend = null)
        {
            Dispose();
            if (!root)
            {
                Debug.LogWarning("ScreenFeedbackService.Initialize called with null root.");
                return;
            }

            view = root.GetComponentInChildren<ScreenFlashView>(true);
            if (!view)
            {
                var go = new GameObject("ScreenFlashView");
                go.transform.SetParent(root, false);
                view = go.AddComponent<ScreenFlashView>();
            }

            view.Backend = backend ?? new UnityFeedbackAnimationBackend();
            view.Ensure();
        }

        public FeedbackHandle Flash(ScreenFlashOptions options)
        {
            if (!view)
            {
                Debug.LogWarning("ScreenFeedbackService.Flash called before Initialize.");
                return FeedbackHandle.Invalid;
            }

            return view.Flash(options);
        }

        public FeedbackHandle Fade(ScreenFlashOptions options)
        {
            return Flash(options);
        }
        public void Stop() { if (view) view.Stop(); }
        public void Dispose() { Stop(); view = null; }
    }
}
