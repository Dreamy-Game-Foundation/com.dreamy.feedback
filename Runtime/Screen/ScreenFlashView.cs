using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Dreamy.Feedback
{
    public sealed class ScreenFlashView : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private Image overlay;

        private CancellationTokenSource flashCancellation;

        public void Ensure()
        {
            if (!canvas)
            {
                canvas = GetComponent<Canvas>() ? GetComponent<Canvas>() : gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = short.MaxValue;
            }

            var raycaster = GetComponent<GraphicRaycaster>();
            if (raycaster)
            {
                raycaster.enabled = false;
            }

            if (!overlay)
            {
                var go = new GameObject("ScreenFlashOverlay", typeof(RectTransform));
                go.transform.SetParent(transform, false);
                overlay = go.AddComponent<Image>();
                overlay.raycastTarget = false;
                var rect = (RectTransform)go.transform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }

            SetAlpha(0f, Color.clear);
        }

        public void Flash(ScreenFlashOptions options, Action complete = null)
        {
            Ensure();
            flashCancellation?.Cancel();
            flashCancellation?.Dispose();
            flashCancellation = new CancellationTokenSource();

            FlashAsync(options, complete, flashCancellation.Token).Forget();
        }

        private void OnDisable()
        {
            flashCancellation?.Cancel();
            flashCancellation?.Dispose();
            flashCancellation = null;
        }

        private async UniTaskVoid FlashAsync(ScreenFlashOptions options, Action complete, CancellationToken cancellationToken)
        {
            await FadeAsync(0f, options.MaxAlpha, options.FadeInDuration, options.Color, cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (options.HoldDuration > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(options.HoldDuration), cancellationToken: cancellationToken).SuppressCancellationThrow();
            }

            await FadeAsync(options.MaxAlpha, 0f, options.FadeOutDuration, options.Color, cancellationToken);
            complete?.Invoke();
        }

        private async UniTask FadeAsync(float from, float to, float duration, Color color, CancellationToken cancellationToken)
        {
            if (duration <= 0f)
            {
                SetAlpha(to, color);
                return;
            }

            var elapsed = 0f;
            while (elapsed < duration)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                elapsed += Time.deltaTime;
                SetAlpha(Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration)), color);
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken).SuppressCancellationThrow();
            }
        }

        private void SetAlpha(float alpha, Color color)
        {
            if (!overlay)
            {
                return;
            }

            color.a = alpha;
            overlay.color = color;
        }
    }
}
