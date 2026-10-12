using UnityEngine;

namespace Dreamy.Feedback
{
    public static class FeedbackUtility
    {
        /// <summary>Maps a UI anchor to the world coordinates of another Canvas root.</summary>
        public static bool TryUiPosition(RectTransform anchor, Transform root, out Vector3 position)
        {
            position = default;
            var sourceCanvas = anchor ? anchor.GetComponentInParent<Canvas>() : null;
            var destinationCanvas = root ? root.GetComponentInParent<Canvas>() : null;
            if (!sourceCanvas || !destinationCanvas || !(root is RectTransform destination)) return false;
            var sourceCamera = sourceCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : sourceCanvas.worldCamera;
            var destinationCamera = destinationCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : destinationCanvas.worldCamera;
            var screen = RectTransformUtility.WorldToScreenPoint(sourceCamera, anchor.TransformPoint(anchor.rect.center));
            return RectTransformUtility.ScreenPointToWorldPointInRectangle(destination, screen, destinationCamera, out position);
        }

        /// <summary>Projects a world point to the transform coordinate space of a UI root.</summary>
        public static Vector3 WorldToUIPosition(Vector3 position, Camera worldCamera, Transform root)
        {
            var canvas = root ? root.GetComponentInParent<Canvas>() : null;
            var rect = root as RectTransform;
            if (!canvas || !rect || !worldCamera) return position;
            var uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            var screen = worldCamera.WorldToScreenPoint(position);
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rect, screen, uiCamera, out var point)) return point;
            return position;
        }

        public static string ToPascalIdentifier(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "Id";
            }

            var result = string.Empty;
            var uppercaseNext = true;
            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                if (!char.IsLetterOrDigit(c))
                {
                    uppercaseNext = true;
                    continue;
                }

                result += uppercaseNext ? char.ToUpperInvariant(c) : c;
                uppercaseNext = false;
            }

            if (string.IsNullOrEmpty(result) || char.IsDigit(result[0]))
            {
                result = "Id" + result;
            }

            return result;
        }
    }
}
