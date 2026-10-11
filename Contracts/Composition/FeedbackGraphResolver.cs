using System;
using System.Collections.Generic;
namespace Dreamy.Feedback
{
    /// <summary>Freeze authoring data at request start, including effects behind a Delay.</summary>
    public static class FeedbackGraphResolver
    {
        public static FeedbackNode Resolve(FeedbackNode root, FeedbackEffectOverrides requestOverrides = default)
            => Copy(root, requestOverrides, new HashSet<FeedbackNode>(), 0);

        private static FeedbackNode Copy(FeedbackNode source, FeedbackEffectOverrides changes, HashSet<FeedbackNode> seen, int depth)
        {
            if (source == null || depth > 16 || seen.Count >= 256 || !seen.Add(source))
                throw new InvalidOperationException("Feedback graph contains a null, cycle/shared node or exceeds its budget.");
            FeedbackNode node;
            if (source.Effect)
            {
                node = source.Effect.Resolve();
            }
            else node = new FeedbackNode { Type = source.Type, Id = source.Id, Duration = source.Duration, Text = source.Text,
                Haptic = source.Haptic, IconFly = source.IconFly, Punch = source.Punch, Shake = source.Shake, Flash = source.Flash,
                VfxSettings = source.VfxSettings, TextSettings = source.TextSettings, DirectAsset = source.DirectAsset };
            node.StepPath = source.StepPath; node.RuntimeContext = source.RuntimeContext;
            node.Required = source.Required;
            source.Overrides.Apply(node);
            changes.Apply(node);
            node.Overrides = source.Overrides;
            node.Overrides.Fields |= changes.Fields;
            if (source.Effect && source.Children != null && source.Children.Count > 0)
                throw new InvalidOperationException("Effect steps cannot contain child nodes.");
            if (source.Children == null) node.Children = null;
            else foreach (var child in source.Children) node.Children.Add(Copy(child, changes, seen, depth + 1));
            return node;
        }
    }
}
