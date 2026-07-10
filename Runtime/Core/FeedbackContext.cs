using UnityEngine;

namespace Dreamy.Feedback
{
    public readonly struct FeedbackContext
    {
        public FeedbackContext(Vector3 worldPosition, Transform parent = null, Transform followTarget = null, Object source = null)
        {
            WorldPosition = worldPosition;
            Parent = parent;
            FollowTarget = followTarget;
            Source = source;
        }

        public Vector3 WorldPosition { get; }
        public Transform Parent { get; }
        public Transform FollowTarget { get; }
        public Object Source { get; }

        public static FeedbackContext At(Vector3 worldPosition) => new FeedbackContext(worldPosition);
    }
}
