using UnityEngine;
namespace Dreamy.Feedback
{
    public interface IUiFeedbackService { FeedbackHandle Punch(Transform target, UiPunchOptions options); }
}
