using System.Collections.Generic;

namespace Dreamy.Feedback.Editor
{
    public static class CameraShakeDatabaseValidator
    {
        public static List<FeedbackValidationIssue> Validate(CameraShakeDatabase database)
        {
            var issues = new List<FeedbackValidationIssue>();
            if (!database)
            {
                issues.Add(new FeedbackValidationIssue(true, "Camera shake database is null."));
                return issues;
            }

            var ids = new HashSet<string>();
            for (var i = 0; i < database.Presets.Count; i++)
            {
                var preset = database.Presets[i];
                if (preset == null)
                {
                    issues.Add(new FeedbackValidationIssue(true, $"Camera shake preset {i} is null."));
                    continue;
                }

                FeedbackValidatorUtility.CheckId(preset.Id, ids, $"Camera shake preset {i}", issues);
                if (preset.Options.Duration <= 0f)
                {
                    issues.Add(new FeedbackValidationIssue(true, $"Camera shake '{preset.Id}' duration must be > 0."));
                }
            }

            return issues;
        }
    }
}
