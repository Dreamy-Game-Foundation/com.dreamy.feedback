using TMPro;
using UnityEngine;
namespace Dreamy.Feedback
{
    public sealed class FloatingTextInstance : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private CanvasGroup canvasGroup;
        private FeedbackHandle animation;
        public FeedbackHandle Play(string value, Vector3 position, Color color, float duration, Vector3 moveOffset, float startScale, float endScale, bool unscaled, IFeedbackAnimationBackend backend)
        {
            animation.Stop();
            if (!text) text = GetComponentInChildren<TMP_Text>(true);
            if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>() ? GetComponent<CanvasGroup>() : gameObject.AddComponent<CanvasGroup>();
            gameObject.SetActive(true); if (text) { text.text = value; text.color = color; text.raycastTarget = false; }
            canvasGroup.blocksRaycasts = false;
            animation = backend.Animate(Mathf.Max(.01f,duration), unscaled, FeedbackEase.OutCubic, t =>
            {
                if (!this) return;
                transform.position = Vector3.Lerp(position, position + moveOffset, t);
                transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, t); canvasGroup.alpha = 1-t;
            });
            return animation;
        }
        private void OnDisable() => animation.Stop();
    }
}
