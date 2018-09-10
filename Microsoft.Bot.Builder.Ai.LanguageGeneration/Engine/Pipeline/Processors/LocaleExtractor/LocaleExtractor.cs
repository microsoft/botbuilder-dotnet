using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.AI.LanguageGeneration.Utilities;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Engine
{
    internal class LocaleExtractor : ILocaleExtractor
    {
        public string ExtractLocale(Activity activity)
        {
            if (activity.Locale != null)
            {
                return activity.Locale;
            }
            else
            {
                return Constants.DefaultLocale;
            }
        }
    }
}
