using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Dreamy.Feedback
{
    public sealed class CameraShakeService : ICameraShakeService
    {
        private CameraShakeDatabase database;
        private Transform cameraTransform;
        private CancellationTokenSource activeCancellation;
        private Vector3 originalLocalPosition;

        public void Initialize(CameraShakeDatabase database, Camera camera)
        {
            this.database = database;
            cameraTransform = camera ? camera.transform : null;
            if (cameraTransform)
            {
                originalLocalPosition = cameraTransform.localPosition;
            }
        }

        public FeedbackHandle Shake(string id)
        {
            if (!database)
            {
                Debug.LogWarning("CameraShakeService.Shake called without database.");
                return FeedbackHandle.Invalid;
            }

            if (!database.TryGet(id, out var preset) || preset == null)
            {
                Debug.LogWarning($"Camera shake id '{id}' was not found.");
                return FeedbackHandle.Invalid;
            }

            return Shake(preset.Options);
        }

        public FeedbackHandle Shake(CameraShakeOptions options)
        {
            if (!cameraTransform)
            {
                Debug.LogWarning("CameraShakeService.Shake called before Initialize.");
                return FeedbackHandle.Invalid;
            }

            Stop();

            activeCancellation = new CancellationTokenSource();
            ShakeAsync(options, activeCancellation.Token).Forget();
            return new FeedbackHandle(true, Stop);
        }

        private void Stop()
        {
            activeCancellation?.Cancel();
            activeCancellation?.Dispose();
            activeCancellation = null;

            if (cameraTransform)
            {
                cameraTransform.localPosition = originalLocalPosition;
            }
        }

        private async UniTaskVoid ShakeAsync(CameraShakeOptions options, CancellationToken cancellationToken)
        {
            var elapsed = 0f;
            var duration = Mathf.Max(0.01f, options.Duration);
            while (elapsed < duration && cameraTransform && !cancellationToken.IsCancellationRequested)
            {
                elapsed += Time.deltaTime;
                var decay = 1f - Mathf.Clamp01(elapsed / duration);
                var x = Mathf.PerlinNoise(Time.time * options.Frequency, 0f) - 0.5f;
                var y = Mathf.PerlinNoise(0f, Time.time * options.Frequency) - 0.5f;
                cameraTransform.localPosition = originalLocalPosition + new Vector3(x, y, 0f) * options.Amplitude * decay;
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken).SuppressCancellationThrow();
            }

            Stop();
        }
    }
}
