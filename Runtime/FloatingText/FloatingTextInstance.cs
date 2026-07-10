using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Dreamy.Feedback
{
    public sealed class FloatingTextInstance : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private CanvasGroup canvasGroup;

        private Action<FloatingTextInstance> releaseAction;
        private CancellationTokenSource animationCancellation;

        public void Initialize(Action<FloatingTextInstance> releaseAction)
        {
            this.releaseAction = releaseAction;
            if (!text)
            {
                text = GetComponentInChildren<TMP_Text>(true);
            }

            if (!canvasGroup)
            {
                canvasGroup = GetComponent<CanvasGroup>() ? GetComponent<CanvasGroup>() : gameObject.AddComponent<CanvasGroup>();
            }
        }

        public void Play(string value, Vector3 position, Color color, float duration, Vector3 moveOffset, float startScale, float endScale)
        {
            animationCancellation?.Cancel();
            animationCancellation?.Dispose();
            animationCancellation = new CancellationTokenSource();

            gameObject.SetActive(true);
            transform.position = position;
            transform.localScale = Vector3.one * startScale;
            if (text)
            {
                text.text = value;
                text.color = color;
            }

            if (canvasGroup)
            {
                canvasGroup.alpha = 1f;
            }

            AnimateAsync(position, duration, moveOffset, startScale, endScale, animationCancellation.Token).Forget();
        }

        private void OnDisable()
        {
            animationCancellation?.Cancel();
            animationCancellation?.Dispose();
            animationCancellation = null;
        }

        private async UniTaskVoid AnimateAsync(Vector3 startPosition, float duration, Vector3 moveOffset, float startScale, float endScale, CancellationToken cancellationToken)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                transform.position = Vector3.Lerp(startPosition, startPosition + moveOffset, t);
                transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, t);
                if (canvasGroup)
                {
                    canvasGroup.alpha = 1f - t;
                }

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken).SuppressCancellationThrow();
            }

            releaseAction?.Invoke(this);
        }
    }
}
