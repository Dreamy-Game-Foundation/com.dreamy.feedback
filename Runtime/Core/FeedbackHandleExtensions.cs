using System.Threading;
using Cysharp.Threading.Tasks;
namespace Dreamy.Feedback
{
    public static class FeedbackHandleExtensions
    {
        /// <summary>Cancels only this wait; use request.CancelWith to cancel playback. Safe for multiple callers.</summary>
        public static UniTask<FeedbackStatus> WaitAsync(this FeedbackHandle handle, CancellationToken cancellation = default)
            => handle.Completion.AsUniTask().AttachExternalCancellation(cancellation);
    }
}

