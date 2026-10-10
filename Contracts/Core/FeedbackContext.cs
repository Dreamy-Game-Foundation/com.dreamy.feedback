using UnityEngine;
namespace Dreamy.Feedback
{
    public readonly struct FeedbackContext
    {
        public FeedbackContext(Vector3 worldPosition, Object source = null, Transform target = null,
            long amount = 0, float intensity = 1, Transform parent = null, Transform followTarget = null,
            Transform iconSource = null, Sprite icon = null)
        {
            WorldPosition = worldPosition; Source = source; Target = target; Amount = amount;
            Intensity = Mathf.Clamp(float.IsNaN(intensity) || float.IsInfinity(intensity) ? 1 : intensity, 0, 4);
            Parent = parent; FollowTarget = followTarget; IconSource = iconSource; Icon = icon;
        }
        public Vector3 WorldPosition { get; }
        public Object Source { get; }
        public Transform Target { get; }
        public Transform IconSource { get; }
        public Sprite Icon { get; }
        public long Amount { get; }
        public float Intensity { get; }
        public Transform Parent { get; }
        public Transform FollowTarget { get; }
        public static FeedbackContext At(Vector3 position) => new FeedbackContext(position);
    }
}
