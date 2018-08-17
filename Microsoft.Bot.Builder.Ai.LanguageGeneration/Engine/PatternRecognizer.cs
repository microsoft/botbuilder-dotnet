using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Bot.Builder.Ai.LanguageGeneration.Helpers
{
    internal enum PatternEnum
    {
        Undefined = 0,
        Template = 1
    }

    internal class PatternRecognizer
    {
        public PatternEnum Recognize(string token)
        {
            //foreach (KeyValuePair<PatternEnum, string> pattern in patterns)
            //{
            //    if (Regex.IsMatch(token, pattern.Value))
            //    {
            //        return pattern.Key;
            //    }
            //}
            //return PatternEnum.Undefined;
        }
    }
}
