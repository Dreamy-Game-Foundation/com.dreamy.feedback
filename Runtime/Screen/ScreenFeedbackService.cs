using UnityEngine;

namespace Dreamy.Feedback
{
    public sealed class ScreenFeedbackService : IScreenFeedbackService
    {
        private ScreenFlashView view;

        public void Initialize(Transform root)
        {
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

            view.Ensure();
        }

        public FeedbackHandle Flash(ScreenFlashOptions options)
        {
            if (!view)
            {
                Debug.LogWarning("ScreenFeedbackService.Flash called before Initialize.");
                return FeedbackHandle.Invalid;
            }

            view.Flash(options);
            return new FeedbackHandle(true, null);
        }

        public FeedbackHandle Fade(ScreenFlashOptions options)
        {
            return Flash(options);
        }
    }
}
