using UnityEngine;
namespace Dreamy.Feedback
{
    public abstract class FeedbackAnimationProvider : MonoBehaviour
    {
        public abstract IFeedbackAnimationBackend CreateBackend();
        public virtual IIconFlyAnimator CreateIconAnimator(IFeedbackAnimationBackend backend) => new UnityIconFlyAnimator(backend);
    }
}
