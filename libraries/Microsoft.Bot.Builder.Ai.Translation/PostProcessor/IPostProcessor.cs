using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Ai.Translation.PostProcessor
{
    /// <summary>
    /// Abstraction for post processor
    /// </summary>
    public interface IPostProcessor
    {
        /// <summary>
        /// Process the specific logic of the implemented post processor.
        /// </summary>
        /// <param name="translatedDocument">Translated document</param>
        /// <param name="currentLanguage">Current source language</param>
        /// <returns></returns>
        PostProcessedDocument Process(TranslatedDocument translatedDocument, string currentLanguage);
    }
}
