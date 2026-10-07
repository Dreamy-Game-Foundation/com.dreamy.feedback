using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Dreamy.Feedback
{
    public sealed class IconFlyInstance : MonoBehaviour
    {
        [SerializeField] private Image image;

        private Action<IconFlyInstance> releaseAction;
        private CancellationTokenSource flyCancellation;

        public void Initialize(Action<IconFlyInstance> releaseAction)
        {
            this.releaseAction = releaseAction;
            if (!image)
            {
                image = GetComponent<Image>() ? GetComponent<Image>() : gameObject.AddComponent<Image>();
            }
        }

        public void Fly(IconFlyOptions options, Vector3 offset)
        {
            flyCancellation?.Cancel();
            flyCancellation?.Dispose();
            flyCancellation = new CancellationTokenSource();

            gameObject.SetActive(true);
            if (image)
            {
                image.sprite = options.Icon;
                image.enabled = options.Icon;
                image.raycastTarget = false;
            }

            var rectTransform = transform as RectTransform;
            if (rectTransform)
            {
                rectTransform.sizeDelta = options.Size == Vector2.zero ? new Vector2(48f, 48f) : options.Size;
            }

            FlyAsync(options, offset, flyCancellation.Token).Forget();
        }

        private void OnDisable()
        {
            flyCancellation?.Cancel();
            flyCancellation?.Dispose();
            flyCancellation = null;
        }

        private async UniTaskVoid FlyAsync(IconFlyOptions options, Vector3 offset, CancellationToken cancellationToken)
        {
            var start = options.StartPosition + offset;
            var end = options.EndPosition;
            var duration = Mathf.Max(0.01f, options.Duration);
            var elapsed = 0f;
            while (elapsed < duration)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                var eased = 1f - Mathf.Pow(1f - t, 3f);
                transform.position = Vector3.Lerp(start, end, eased);
                transform.localScale = Vector3.one * Mathf.Lerp(0.85f, 1f, t);
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken).SuppressCancellationThrow();
            }

            if (!cancellationToken.IsCancellationRequested) releaseAction?.Invoke(this);
        }
    }
}
