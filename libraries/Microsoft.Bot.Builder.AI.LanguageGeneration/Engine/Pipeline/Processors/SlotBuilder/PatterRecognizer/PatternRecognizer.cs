using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Helpers
{
    internal static class PatternRecognizer
    {
        private static readonly string _templatePattern = @"\[(.*?)\]";
        public static List<string> Recognize(string token)
        {
            if (Regex.IsMatch(token, _templatePattern))
            {
                var detectedPatterns = new List<string>();
                var matches = Regex.Matches(token, _templatePattern);
                foreach (Match match in matches)
                {
                    if (match.Index > 0 && IsEscapedPattern(token[match.Index - 1] + match.Value))
                    {
                        continue;
                    }
                    detectedPatterns.Add(match.Value);
                }
                return detectedPatterns;
            }

            return new List<string>();
        }

        private static bool IsEscapedPattern(string pattern) => (pattern.StartsWith(@"\") && pattern.EndsWith(@"\]"));
    }
}
