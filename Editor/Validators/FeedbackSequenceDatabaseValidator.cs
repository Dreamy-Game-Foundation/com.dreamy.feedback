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

                    if (action.Type == FeedbackSequenceActionType.FloatingText && string.IsNullOrWhiteSpace(action.Text))
                    {
                        issues.Add(new FeedbackValidationIssue(true, $"Sequence '{entry.Id}' floating text action {j} is missing text."));
                    }
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
