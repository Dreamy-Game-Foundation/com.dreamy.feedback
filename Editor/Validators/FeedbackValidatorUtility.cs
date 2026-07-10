using System.Collections.Generic;
using UnityEngine;

namespace Dreamy.Feedback.Editor
{
    internal static class FeedbackValidatorUtility
    {
        public static void CheckId(string id, HashSet<string> ids, string label, List<FeedbackValidationIssue> issues)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                issues.Add(new FeedbackValidationIssue(true, $"{label} has empty id."));
                return;
            }

            if (!ids.Add(id))
            {
                issues.Add(new FeedbackValidationIssue(true, $"{label} has duplicate id '{id}'."));
            }
        }

        public static void LogIssues(string title, IReadOnlyList<FeedbackValidationIssue> issues)
        {
            if (issues.Count == 0)
            {
                Debug.Log($"{title}: OK");
                return;
            }

            for (var i = 0; i < issues.Count; i++)
            {
                if (issues[i].IsError)
                {
                    Debug.LogError($"{title}: {issues[i].Message}");
                }
                else
                {
                    Debug.LogWarning($"{title}: {issues[i].Message}");
                }
            }
        }
    }
}
