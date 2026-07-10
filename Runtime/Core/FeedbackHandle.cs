using System;

namespace Dreamy.Feedback
{
    public readonly struct FeedbackHandle
    {
        private readonly Action stopAction;

        public FeedbackHandle(bool isValid, Action stopAction)
        {
            IsValid = isValid;
            this.stopAction = stopAction;
        }

        public bool IsValid { get; }

        public void Stop()
        {
            stopAction?.Invoke();
        }

        public static FeedbackHandle Invalid => new FeedbackHandle(false, null);
    }
}
