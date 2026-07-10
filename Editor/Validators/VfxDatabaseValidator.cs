using System.Collections.Generic;

namespace Dreamy.Feedback.Editor
{
    public static class VfxDatabaseValidator
    {
        public static List<FeedbackValidationIssue> Validate(VfxDatabase database)
        {
            var issues = new List<FeedbackValidationIssue>();
            if (!database)
            {
                issues.Add(new FeedbackValidationIssue(true, "VFX database is null."));
                return issues;
            }

            var ids = new HashSet<string>();
            for (var i = 0; i < database.Entries.Count; i++)
            {
                var entry = database.Entries[i];
                if (entry == null)
                {
                    issues.Add(new FeedbackValidationIssue(true, $"VFX entry {i} is null."));
                    continue;
                }

                FeedbackValidatorUtility.CheckId(entry.Id, ids, $"VFX entry {i}", issues);
                if (!entry.Prefab)
                {
                    issues.Add(new FeedbackValidationIssue(true, $"VFX '{entry.Id}' prefab is null."));
                }
                else if (!entry.Prefab.GetComponent<VfxInstance>() && !entry.Prefab.GetComponentInChildren<UnityEngine.ParticleSystem>(true))
                {
                    issues.Add(new FeedbackValidationIssue(false, $"VFX '{entry.Id}' prefab has no ParticleSystem or VfxInstance."));
                }

                if (entry.PrewarmCount < 0)
                {
                    issues.Add(new FeedbackValidationIssue(true, $"VFX '{entry.Id}' prewarmCount is negative."));
                }

                if (entry.DespawnMode == VfxDespawnMode.FixedLifetime && entry.FixedLifetime <= 0f)
                {
                    issues.Add(new FeedbackValidationIssue(true, $"VFX '{entry.Id}' fixedLifetime must be > 0."));
                }
            }

            return issues;
        }
    }
}
