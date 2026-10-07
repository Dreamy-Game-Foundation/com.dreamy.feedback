using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Dreamy.Feedback
{
    public sealed class CameraShakeService : ICameraShakeService, IDisposable
    {
        private CameraShakeDatabase database;
        private Transform cameraTransform;
        private CancellationTokenSource activeCancellation;
        private Vector3 originalLocalPosition;

        public void Initialize(CameraShakeDatabase database, Camera camera)
        {
            Stop();
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

            originalLocalPosition = cameraTransform.localPosition;
            var session = new CancellationTokenSource();
            activeCancellation = session;
            ShakeAsync(options, session).Forget();
            return new FeedbackHandle(true, () => { if (ReferenceEquals(activeCancellation, session)) Stop(); });
        }

        public void Stop()
        {
            if (activeCancellation == null) return;
            activeCancellation.Cancel();
            activeCancellation?.Dispose();
            activeCancellation = null;

            if (cameraTransform)
            {
                cameraTransform.localPosition = originalLocalPosition;
            }
        }

        public void Dispose() { Stop(); database = null; cameraTransform = null; }

        private async UniTaskVoid ShakeAsync(CameraShakeOptions options, CancellationTokenSource session)
        {
            var cancellationToken = session.Token;
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

            if (ReferenceEquals(activeCancellation, session)) Stop();
        }
    }
}
