namespace Dreamy.Feedback.Editor
{
    public readonly struct FeedbackValidationIssue
    {
        public FeedbackValidationIssue(bool isError, string message)
        {
            IsError = isError;
            Message = message;
        }

        public bool IsError { get; }
        public string Message { get; }
    }
}
