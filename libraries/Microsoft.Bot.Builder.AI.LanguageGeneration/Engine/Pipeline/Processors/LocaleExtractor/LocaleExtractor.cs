using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.AI.LanguageGeneration.Utilities;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Engine
{
    /// <summary>
    /// The cocrete class for extracting locale from the <see cref="Activity"/> object.
    /// </summary>
    internal class LocaleExtractor : ILocaleExtractor
    {
        /// <summary>
        /// Extract locale method.
        /// </summary>
        /// <param name="activity"><see cref="Activity"/> object.</param>
        /// <returns>A <see cref="string"/> which evaluates to the user locale.</returns>
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
