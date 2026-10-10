using UnityEngine;
namespace Dreamy.Feedback
{
    public interface IIconFlyAnimator
    {
        FeedbackHandle Animate(Transform icon, CanvasGroup opacity, IconFlyOptions options, Vector3 spread, int index);
    }
}
