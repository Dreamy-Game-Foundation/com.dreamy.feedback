using UnityEngine;

namespace Dreamy.Feedback
{
    public struct VfxPlayOptions
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Transform Parent;
        public Transform FollowTarget;
        public bool UseLocalPosition;

        public static VfxPlayOptions At(Vector3 position)
        {
            return new VfxPlayOptions
            {
                Position = position,
                Rotation = Quaternion.identity
            };
        }
    }
}
