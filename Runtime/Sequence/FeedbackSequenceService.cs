using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Dreamy.Feedback
{
    public sealed class FeedbackSequenceService : IFeedbackSequenceService, IDisposable
    {
        private FeedbackSequenceDatabase database;
        private FeedbackSequenceServices services;
        private CancellationTokenSource activeCancellation;

        public void Initialize(FeedbackSequenceDatabase database, FeedbackSequenceServices services)
        {
            Stop();
            this.database = database;
            this.services = services ?? new FeedbackSequenceServices();
        }

        public FeedbackHandle Play(string id, FeedbackContext context)
        {
            if (!database)
            {
                Debug.LogWarning("FeedbackSequenceService.Play called before Initialize.");
                return FeedbackHandle.Invalid;
            }

            if (!database.TryGet(id, out var entry) || entry == null)
            {
                Debug.LogWarning($"Feedback sequence id '{id}' was not found.");
                return FeedbackHandle.Invalid;
            }

            Stop();
            var session = new CancellationTokenSource();
            activeCancellation = session;
            PlayAsync(entry, context, session).Forget();
            return new FeedbackHandle(true, () => { if (ReferenceEquals(activeCancellation, session)) Stop(); });
        }

        public void Stop()
        {
            activeCancellation?.Cancel();
            activeCancellation?.Dispose();
            activeCancellation = null;
        }

        public void Dispose() { Stop(); database = null; services = null; }

        private async UniTaskVoid PlayAsync(FeedbackSequenceEntry entry, FeedbackContext context, CancellationTokenSource session)
        {
            var cancellationToken = session.Token;
            try
            {
                for (var i = 0; i < entry.Actions.Count; i++)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    var action = entry.Actions[i];
                    if (action == null)
                    {
                        continue;
                    }

                    if (action.Delay > 0f)
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(action.Delay), cancellationToken: cancellationToken).SuppressCancellationThrow();
                    }

                    if (cancellationToken.IsCancellationRequested) return;
                    PlayAction(action, context);
                }
            }
            finally { if (ReferenceEquals(activeCancellation, session)) Stop(); }
        }

        private void PlayAction(FeedbackSequenceAction action, FeedbackContext context)
        {
            switch (action.Type)
            {
                case FeedbackSequenceActionType.Vfx:
                    if (services.Vfx == null)
                    {
                        Debug.LogWarning("Feedback sequence skipped VFX action because IVfxService is missing.");
                        return;
                    }

                    services.Vfx.Play(action.Id, new VfxPlayOptions
                    {
                        Position = context.WorldPosition,
                        Parent = context.Parent,
                        FollowTarget = context.FollowTarget
                    });
                    break;
                case FeedbackSequenceActionType.Haptic:
                    if (services.Haptic == null)
                    {
                        Debug.LogWarning("Feedback sequence skipped haptic action because IHapticService is missing.");
                        return;
                    }

                    services.Haptic.Play(action.HapticType);
                    break;
                case FeedbackSequenceActionType.FloatingText:
                    if (services.FloatingText == null)
                    {
                        Debug.LogWarning("Feedback sequence skipped floating text action because IFloatingTextService is missing.");
                        return;
                    }

                    services.FloatingText.Play(action.Text, context.WorldPosition, FloatingTextOptions.Style(action.Id));
                    break;
                case FeedbackSequenceActionType.IconFly:
                    if (services.IconFly == null)
                    {
                        Debug.LogWarning("Feedback sequence skipped icon fly action because IIconFlyService is missing.");
                        return;
                    }

                    services.IconFly.Fly(action.IconFlyOptions);
                    break;
                case FeedbackSequenceActionType.ScreenFlash:
                    if (services.ScreenFeedback == null)
                    {
                        Debug.LogWarning("Feedback sequence skipped screen flash action because IScreenFeedbackService is missing.");
                        return;
                    }

                    services.ScreenFeedback.Flash(action.ScreenFlashOptions);
                    break;
                case FeedbackSequenceActionType.CameraShake:
                    if (services.CameraShake == null)
                    {
                        Debug.LogWarning("Feedback sequence skipped camera shake action because ICameraShakeService is missing.");
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(action.Id))
                    {
                        services.CameraShake.Shake(action.CameraShakeOptions);
                    }
                    else
                    {
                        services.CameraShake.Shake(action.Id);
                    }
                    break;
                case FeedbackSequenceActionType.Delay:
                    break;
            }
        }
    }
}
