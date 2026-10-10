using System;
using System.Threading.Tasks;
namespace Dreamy.Feedback
{
    public enum FeedbackStatus { Invalid, Running, Completed, Stopped, Faulted }
    public readonly struct FeedbackHandle
    {
        private readonly FeedbackPlayback playback;
        public FeedbackHandle(FeedbackPlayback playback) { this.playback = playback; }
        public bool IsValid => playback != null;
        public bool IsRunning => Status == FeedbackStatus.Running;
        public FeedbackStatus Status => playback?.Status ?? FeedbackStatus.Invalid;
        public Task<FeedbackStatus> Completion => playback?.Completion ?? Task.FromResult(FeedbackStatus.Invalid);
        public void Stop() => playback?.Stop();
        public void Complete() => playback?.Complete();
        public static FeedbackHandle Invalid => default;
        public static FeedbackHandle Completed { get { var p = new FeedbackPlayback(); p.Finish(); return p.Handle; } }
    }
    /// <summary>Shared state for a single lease. Backends must finish it on every terminal path.</summary>
    public sealed class FeedbackPlayback
    {
        private readonly TaskCompletionSource<FeedbackStatus> completion = new TaskCompletionSource<FeedbackStatus>();
        private Action stop, complete;
        public FeedbackStatus Status { get; private set; } = FeedbackStatus.Running;
        public Task<FeedbackStatus> Completion => completion.Task;
        public FeedbackHandle Handle => new FeedbackHandle(this);
        public void Bind(Action stop, Action complete) { this.stop = stop; this.complete = complete; }
        public void Stop() => End(FeedbackStatus.Stopped, stop);
        public void Complete() => End(FeedbackStatus.Completed, complete);
        public void Finish() => End(FeedbackStatus.Completed, null);
        public void Fail() => End(FeedbackStatus.Faulted, stop);
        private void End(FeedbackStatus result, Action action)
        {
            if (Status != FeedbackStatus.Running) return;
            Status = result;
            try { action?.Invoke(); }
            catch { Status = FeedbackStatus.Faulted; throw; }
            finally { stop = complete = null; completion.TrySetResult(Status); }
        }
    }
}
