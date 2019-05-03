using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Defines interface for a LG system to bind to text.
    /// </summary>
    public interface ILanguageGenerator
    {
        /// <summary>
        /// Method to bind data to string
        /// </summary>
        /// <param name="locale">locale such as "en-gb".</param>
        /// <param name="inlineTemplate">inline template (overrides any hierarchy).</param>
        /// <param name="id">property on the types to evaluate for fallback.</param>
        /// <param name="data">data to bind to.</param>
        /// <param name="types">array of prefixes to try.</param>
        /// <param name="tags">array of tags to capture context.</param>
        /// <returns>text or errors</returns>
        Task<string> Generate(string locale, string inlineTemplate, string id, object data, string[] types, string[] tags);
    }
}
