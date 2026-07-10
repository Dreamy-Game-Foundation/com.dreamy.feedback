using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Dreamy.Feedback
{
    public class VfxInstance : MonoBehaviour
    {
        private Action<VfxInstance> releaseAction;
        private CancellationTokenSource followCancellation;

        public void Initialize(Action<VfxInstance> releaseAction)
        {
            this.releaseAction = releaseAction;
        }

        public virtual void Play()
        {
            gameObject.SetActive(true);
        }

        public virtual void Stop()
        {
            releaseAction?.Invoke(this);
        }

        public void Follow(Transform target)
        {
            if (!target)
            {
                return;
            }

            followCancellation?.Cancel();
            followCancellation?.Dispose();
            followCancellation = new CancellationTokenSource();
            FollowAsync(target, followCancellation.Token).Forget();
        }

        protected virtual void OnDisable()
        {
            followCancellation?.Cancel();
            followCancellation?.Dispose();
            followCancellation = null;
        }

        private async UniTaskVoid FollowAsync(Transform target, CancellationToken cancellationToken)
        {
            while (target && !cancellationToken.IsCancellationRequested)
            {
                transform.position = target.position;
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken).SuppressCancellationThrow();
            }
        }
    }
}
