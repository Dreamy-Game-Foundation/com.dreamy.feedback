using UnityEngine;
using UnityEngine.UI;

namespace Dreamy.Feedback
{
    [System.Serializable]
    public struct IconFlyOptions
    {
        public Sprite Icon;
        public Vector3 StartPosition;
        public Vector3 EndPosition;
        public int Count;
        public float Duration;
        public float Spread;
        public Vector2 Size;

        public static IconFlyOptions Create(Sprite icon, Vector3 startPosition, Vector3 endPosition)
        {
            return new IconFlyOptions
            {
                Icon = icon,
                StartPosition = startPosition,
                EndPosition = endPosition,
                Count = 1,
                Duration = 0.6f,
                Spread = 40f,
                Size = new Vector2(48f, 48f)
            };
        }
    }
}
