using System;
using System.Threading;
using UnityEngine;
namespace Dreamy.Feedback
{
    [Serializable]
    public struct IconFlyFeedbackSettings
    {
        public Sprite Icon;
        public int Count;
        public float Duration, Spread, Stagger, ArcHeight, Spin;
        public Vector2 Size;
        public IconFlyStyle Style;
        public FeedbackEase Ease;
        public static IconFlyFeedbackSettings Default => From(IconFlyOptions.Create(null, Vector3.zero, Vector3.zero));
        public static IconFlyFeedbackSettings From(IconFlyOptions value) => new IconFlyFeedbackSettings
        { Icon = value.Icon, Count = value.Count, Duration = value.Duration, Spread = value.Spread, Stagger = value.Stagger, ArcHeight = value.ArcHeight, Spin = value.Spin, Size = value.Size, Style = value.Style, Ease = value.Ease };
        public IconFlyOptions ToOptions() => new IconFlyOptions
        { Icon = Icon, Count = Count, Duration = Duration, Spread = Spread, Stagger = Stagger, ArcHeight = ArcHeight, Spin = Spin, Size = Size, Style = Style, Ease = Ease, UnscaledTime = true };
    }
    public struct IconFlyPlayArgs
    {
        public Transform Source, Target;
        public Sprite Icon;
        public int? Count;
        public UnityEngine.Object Owner;
        public CancellationToken Cancellation;
        public bool? UnscaledTime;
    }
}
