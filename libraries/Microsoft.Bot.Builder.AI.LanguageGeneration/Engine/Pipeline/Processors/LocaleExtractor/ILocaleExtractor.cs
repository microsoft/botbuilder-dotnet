using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Engine
{
    /// <summary>
    /// The blueprint for extracting locale from the <see cref="Activity"/> object.
    /// </summary>
    internal interface ILocaleExtractor
    {
        /// <summary>
        /// Extract locale method.
        /// </summary>
        /// <param name="activity"><see cref="Activity"/> object.</param>
        /// <returns>A <see cref="string"/> which evaluates to the user locale.</returns>
        string ExtractLocale(Activity activity);
    }
}
