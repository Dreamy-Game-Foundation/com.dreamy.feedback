using System.Collections.Generic;
using UnityEngine;
namespace Dreamy.Feedback
{
    public static class FeedbackGraphValidation
    {
        public static string Validate(FeedbackDefinition definition)
        {
            if (!definition || string.IsNullOrWhiteSpace(definition.Id)) return "Feedback definition requires a stable ID.";
            var seen = new HashSet<FeedbackNode>(); int count = 0;
            return Visit(definition.Root, seen, 0, ref count);
        }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        private static string Visit(FeedbackNode node, HashSet<FeedbackNode> seen, int depth, ref int count)
        {
            if (node == null) return "Feedback graph contains a null node.";
            if (depth > 16 || ++count > 256) return "Feedback graph exceeds depth 16 / 256 nodes.";
            if (!seen.Add(node)) return "Feedback graph contains a cycle or shared node.";
            if (!System.Enum.IsDefined(typeof(FeedbackNodeType), node.Type)) return "Unknown feedback node type.";
            if (float.IsNaN(node.Duration) || float.IsInfinity(node.Duration) || node.Duration < 0) return "Node duration must be finite and non-negative.";
            if ((node.Type == FeedbackNodeType.Vfx || node.Type == FeedbackNodeType.FloatingText) && string.IsNullOrWhiteSpace(node.Id)) return "VFX/text nodes require a preset ID.";
            if (node.Type == FeedbackNodeType.Vfx && node.Duration <= 0) return "Composite VFX requires a positive maximum duration to bound manual/looping effects.";
            if (node.Type == FeedbackNodeType.IconFly && (!node.IconFly.Icon || node.IconFly.Count < 1 || node.IconFly.Duration <= 0)) return "Icon fly requires sprite, count and positive duration.";
            if (node.Type == FeedbackNodeType.IconFly && (!Finite(node.IconFly.Duration) || !Finite(node.IconFly.Stagger) || node.IconFly.Stagger < 0 || !Finite(node.IconFly.Spread) || !Finite(node.IconFly.ArcHeight) || !Finite(node.IconFly.Spin))) return "Icon fly parameters must be finite; stagger must be non-negative.";
            if (node.Type == FeedbackNodeType.UiPunch && (!Finite(node.Punch.Duration) || node.Punch.Duration <= 0 || !Finite(node.Punch.Strength))) return "UI punch requires finite strength and positive duration.";
            if (node.Type == FeedbackNodeType.CameraShake && (!Finite(node.Shake.Duration) || node.Shake.Duration <= 0 || !Finite(node.Shake.Amplitude) || !Finite(node.Shake.Frequency))) return "Camera shake parameters must be finite with positive duration.";
            if (node.Type == FeedbackNodeType.ScreenFlash && (!Finite(node.Flash.MaxAlpha) || node.Flash.MaxAlpha < 0 || node.Flash.MaxAlpha > 1 || !Finite(node.Flash.FadeInDuration) || node.Flash.FadeInDuration < 0 || !Finite(node.Flash.FadeOutDuration) || node.Flash.FadeOutDuration < 0 || !Finite(node.Flash.HoldDuration) || node.Flash.HoldDuration < 0)) return "Screen flash parameters must be finite with non-negative timing and alpha in [0,1].";
            if (node.Type == FeedbackNodeType.Sequence || node.Type == FeedbackNodeType.Parallel)
            {
                if (node.Children == null || node.Children.Count == 0) return "Composite group requires children.";
                foreach (var child in node.Children) { var error = Visit(child, seen, depth + 1, ref count); if (error != null) return error; }
            }
            else if (node.Children != null && node.Children.Count > 0) return "Only Sequence/Parallel may contain children.";
            return null;
        }
    }
}
