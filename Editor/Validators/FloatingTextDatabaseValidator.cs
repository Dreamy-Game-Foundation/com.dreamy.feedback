using System.Collections.Generic;

namespace Dreamy.Feedback.Editor
{
    public static class FloatingTextDatabaseValidator
    {
        public static List<FeedbackValidationIssue> Validate(FloatingTextDatabase database)
        {
            var issues = new List<FeedbackValidationIssue>();
            if (!database)
            {
                issues.Add(new FeedbackValidationIssue(true, "Floating text database is null."));
                return issues;
            }

            var ids = new HashSet<string>();
            for (var i = 0; i < database.Entries.Count; i++)
            {
                var entry = database.Entries[i];
                if (entry == null)
                {
                    issues.Add(new FeedbackValidationIssue(true, $"Floating text entry {i} is null."));
                    continue;
                }

                FeedbackValidatorUtility.CheckId(entry.Id, ids, $"Floating text entry {i}", issues);
                if (!entry.Prefab)
                {
                    issues.Add(new FeedbackValidationIssue(true, $"Floating text '{entry.Id}' prefab is null."));
                }

                if (entry.PrewarmCount < 0)
                {
                    issues.Add(new FeedbackValidationIssue(true, $"Floating text '{entry.Id}' prewarmCount is negative."));
                }

                if (entry.Duration <= 0f)
                {
                    issues.Add(new FeedbackValidationIssue(true, $"Floating text '{entry.Id}' duration must be > 0."));
                }
            }

            return issues;
        }
    }
}
