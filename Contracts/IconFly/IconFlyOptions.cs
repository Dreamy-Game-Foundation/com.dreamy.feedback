using UnityEngine;
namespace Dreamy.Feedback
{
    public enum IconFlyStyle { Straight, Arc, ScatterMagnet, Fountain, Spiral }
    [System.Serializable]
    public struct IconFlyOptions
    {
        public Sprite Icon;
        public Vector3 StartPosition, EndPosition;
        public int Count;
        public float Duration, Spread, Stagger, ArcHeight, Spin;
        public Vector2 Size;
        public IconFlyStyle Style;
        public FeedbackEase Ease;
        public bool UnscaledTime;
        [System.NonSerialized] public Transform Target;
        public static IconFlyOptions Create(Sprite icon, Vector3 start, Vector3 end) => new IconFlyOptions
        { Icon = icon, StartPosition = start, EndPosition = end, Count = 1, Duration = .65f, Spread = 65,
          Size = new Vector2(48,48), Stagger = .035f, ArcHeight = 140, Spin = 180, Style = IconFlyStyle.ScatterMagnet, Ease = FeedbackEase.InOutSine, UnscaledTime = true };
    }
}
