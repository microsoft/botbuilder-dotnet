using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Utilities
{
    /// <summary>
    /// Constants used across the language generation pipelines
    /// </summary>
    internal class Constants
    {
        public static string DefaultLocale = "en-US";
        public static string DefaultTokenGenerationEndpoint = "https://api.cognitive.microsoft.com/sts/v1.0/issueToken";
    }
}
