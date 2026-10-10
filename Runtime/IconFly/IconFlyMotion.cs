using UnityEngine;
namespace Dreamy.Feedback
{
    /// <summary>Local canvas units keep trajectory/size stable across CanvasScaler resolutions.</summary>
    public static class IconFlyMotion
    {
        public static void Apply(Transform icon, CanvasGroup opacity, IconFlyOptions options, Vector3 spread, float t)
        {
            var parent = icon.parent;
            Vector3 start = parent.InverseTransformPoint(options.StartPosition);
            Vector3 end = parent.InverseTransformPoint(options.Target ? options.Target.position : options.EndPosition);
            float eased = UnityFeedbackAnimationBackend.Evaluate(t, options.Ease);
            Vector3 point = Vector3.LerpUnclamped(start, end, eased);
            switch (options.Style)
            {
                case IconFlyStyle.Arc: point += Vector3.up * (4 * t * (1-t) * options.ArcHeight); break;
                case IconFlyStyle.ScatterMagnet:
                    if (t < .3f) point = Vector3.Lerp(start, start + spread, UnityFeedbackAnimationBackend.Evaluate(t / .3f, FeedbackEase.OutCubic));
                    else point = Vector3.Lerp(start + spread, end, UnityFeedbackAnimationBackend.Evaluate((t-.3f)/.7f, FeedbackEase.InCubic));
                    break;
                case IconFlyStyle.Fountain:
                    point += (Vector3.up * options.ArcHeight + spread) * Mathf.Sin(t * Mathf.PI); break;
                case IconFlyStyle.Spiral:
                    float angle = t * Mathf.PI * 4 + Mathf.Atan2(spread.y, spread.x);
                    point += new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * options.Spread * Mathf.Sin(t * Mathf.PI); break;
            }
            icon.localPosition = point;
            icon.localRotation = Quaternion.Euler(0, 0, options.Spin * t);
            icon.localScale = Vector3.one * (t < .2f ? Mathf.Lerp(.45f, 1.15f, t/.2f) : Mathf.Lerp(1.15f, .7f, (t-.2f)/.8f));
            opacity.alpha = t > .92f ? (1-t)/.08f : Mathf.Clamp01(t/.08f);
        }
    }
}
