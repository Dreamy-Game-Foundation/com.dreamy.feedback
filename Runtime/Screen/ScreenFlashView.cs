using System;
using UnityEngine;
using UnityEngine.UI;
namespace Dreamy.Feedback
{
    public sealed class ScreenFlashView : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private Image overlay;
        private FeedbackHandle active;
        public IFeedbackAnimationBackend Backend { get; set; } = new UnityFeedbackAnimationBackend();
        public void Ensure()
        {
            if (!canvas) { canvas = GetComponent<Canvas>() ? GetComponent<Canvas>() : gameObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = short.MaxValue; }
            var raycaster = GetComponent<GraphicRaycaster>(); if (raycaster) raycaster.enabled = false;
            if (!overlay)
            {
                var go = new GameObject("ScreenFlashOverlay", typeof(RectTransform)); go.transform.SetParent(transform,false);
                overlay = go.AddComponent<Image>(); overlay.raycastTarget = false;
                var rect = (RectTransform)go.transform; rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
            }
            SetAlpha(0,Color.clear);
        }
        public FeedbackHandle Flash(ScreenFlashOptions options, Action complete = null)
        {
            Ensure(); Stop(); var state = new FeedbackPlayback(); FeedbackHandle animation = default;
            Action reset = () => { animation.Stop(); SetAlpha(0,Color.clear); };
            state.Bind(reset, () => { reset(); complete?.Invoke(); }); active = state.Handle;
            float total = Mathf.Max(.01f, options.FadeInDuration + options.HoldDuration + options.FadeOutDuration);
            animation = Backend.Animate(total,options.UnscaledTime,FeedbackEase.Linear,t =>
            {
                if (!this || !isActiveAndEnabled) { state.Stop(); return; }
                float elapsed = t * total;
                float alpha = elapsed < options.FadeInDuration ? elapsed / Mathf.Max(.001f,options.FadeInDuration)
                    : elapsed < options.FadeInDuration + options.HoldDuration ? 1
                    : 1 - (elapsed - options.FadeInDuration - options.HoldDuration) / Mathf.Max(.001f, options.FadeOutDuration);
                SetAlpha(Mathf.Clamp01(alpha) * options.MaxAlpha,options.Color);
                if (t >= 1) { reset(); state.Finish(); complete?.Invoke(); }
            });
            return state.Handle;
        }
        public void Stop() { active.Stop(); SetAlpha(0,Color.clear); }
        private void OnDisable() => Stop();
        private void SetAlpha(float alpha, Color color) { if (overlay) { color.a = alpha; overlay.color = color; } }
    }
}
