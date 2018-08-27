using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Bot.Builder.Ai.LanguageGeneration.Helpers
{
    internal static class PatternRecognizer
    {
        private static readonly string _templatePattern = @"\{([^}]+)\}|\[([^\]]+)\]";
        public static IList<string> Recognize(string token)
        {
            if (Regex.IsMatch(token, _templatePattern))
            {
                var detectedPatterns = new List<string>();
                var matches = Regex.Matches(token, _templatePattern);
                foreach (Match match in matches)
                {
                    detectedPatterns.Add(match.Value);
                }
                return detectedPatterns;
            }
            else
            {
                return new List<string>();
            }
        }
    }
}
