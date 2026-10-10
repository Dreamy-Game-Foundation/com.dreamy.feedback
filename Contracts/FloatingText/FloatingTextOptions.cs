using UnityEngine;

namespace Dreamy.Feedback
{
    public struct FloatingTextOptions
    {
        public string StyleId;
        public bool UnscaledTime;
        public Color? Color;
        public float? Duration;
        public Vector3? MoveOffset;
        public float? StartScale;
        public float? EndScale;

        public static FloatingTextOptions Style(string styleId)
        {
            return new FloatingTextOptions { StyleId = styleId };
        }
    }
}
