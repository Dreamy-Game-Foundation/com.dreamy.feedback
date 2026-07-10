using UnityEngine;

namespace Dreamy.Feedback
{
    public static class FeedbackUtility
    {
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
