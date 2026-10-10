using System.Collections.Generic;
namespace Dreamy.Feedback.Editor
{
    public static class FeedbackDefinitionValidator
    {
        public static List<FeedbackValidationIssue> Validate(FeedbackDefinition definition)
        {
            var issues = new List<FeedbackValidationIssue>();
            string error = FeedbackGraphValidation.Validate(definition);
            if (error != null) issues.Add(new FeedbackValidationIssue(true,error));
            return issues;
        }
    }
}
