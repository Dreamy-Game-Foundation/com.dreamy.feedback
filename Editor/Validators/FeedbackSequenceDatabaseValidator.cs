using System.Collections.Generic;

namespace Dreamy.Feedback.Editor
{
    public static class FeedbackSequenceDatabaseValidator
    {
        public static List<FeedbackValidationIssue> Validate(FeedbackSequenceDatabase database)
        {
            var issues = new List<FeedbackValidationIssue>();
            if (!database)
            {
                issues.Add(new FeedbackValidationIssue(true, "Feedback sequence database is null."));
                return issues;
            }

            var ids = new HashSet<string>();
            for (var i = 0; i < database.Entries.Count; i++)
            {
                var entry = database.Entries[i];
                if (entry == null)
                {
                    issues.Add(new FeedbackValidationIssue(true, $"Sequence entry {i} is null."));
                    continue;
                }

                FeedbackValidatorUtility.CheckId(entry.Id, ids, $"Sequence entry {i}", issues);
                if (entry.Actions.Count == 0) issues.Add(new FeedbackValidationIssue(true, $"Sequence '{entry.Id}' has no actions."));
                for (var j = 0; j < entry.Actions.Count; j++)
                {
                    var action = entry.Actions[j];
                    if (action == null)
                    {
                        issues.Add(new FeedbackValidationIssue(true, $"Sequence '{entry.Id}' action {j} is null."));
                        continue;
                    }

                    if (RequiresId(action.Type) && string.IsNullOrWhiteSpace(action.Id))
                    {
                        issues.Add(new FeedbackValidationIssue(true, $"Sequence '{entry.Id}' action {j} is missing id."));
                    }
                    if (action.Delay < 0 || float.IsNaN(action.Delay) || float.IsInfinity(action.Delay))
                        issues.Add(new FeedbackValidationIssue(true, $"Sequence '{entry.Id}' action {j} delay must be finite and non-negative."));
                    if (action.Type == FeedbackSequenceActionType.IconFly && (!action.IconFlyOptions.Icon || action.IconFlyOptions.Count < 1 || action.IconFlyOptions.Duration <= 0))
                        issues.Add(new FeedbackValidationIssue(true, $"Sequence '{entry.Id}' icon action {j} requires a sprite, count and positive duration."));
                    if (action.Type == FeedbackSequenceActionType.ScreenFlash && (action.ScreenFlashOptions.MaxAlpha < 0 || action.ScreenFlashOptions.MaxAlpha > 1))
                        issues.Add(new FeedbackValidationIssue(true, $"Sequence '{entry.Id}' screen action {j} alpha must be in [0, 1]."));

                    if (action.Type == FeedbackSequenceActionType.FloatingText && string.IsNullOrWhiteSpace(action.Text))
                    {
                        issues.Add(new FeedbackValidationIssue(true, $"Sequence '{entry.Id}' floating text action {j} is missing text."));
                    }
                }
            }

            return issues;
        }

        public static List<FeedbackValidationIssue> Validate(FeedbackSequenceDatabase database,
            VfxDatabase vfx, FloatingTextDatabase text, CameraShakeDatabase shake)
        {
            var issues = Validate(database);
            if (!database) return issues;
            foreach (var entry in database.Entries)
            {
                if (entry == null) continue;
                foreach (var action in entry.Actions)
                {
                    if (action == null) continue;
                    bool missing = action.Type == FeedbackSequenceActionType.Vfx && (!vfx || !vfx.TryGet(action.Id, out _))
                        || action.Type == FeedbackSequenceActionType.FloatingText && (!text || !text.TryGet(action.Id, out _))
                        || action.Type == FeedbackSequenceActionType.CameraShake && !string.IsNullOrWhiteSpace(action.Id) && (!shake || !shake.TryGet(action.Id, out _));
                    if (missing) issues.Add(new FeedbackValidationIssue(true, $"Sequence '{entry.Id}' references missing {action.Type} id '{action.Id}'."));
                }
            }
            return issues;
        }

        private static bool RequiresId(FeedbackSequenceActionType type)
        {
            return type == FeedbackSequenceActionType.Vfx || type == FeedbackSequenceActionType.FloatingText;
        }
    }
}
