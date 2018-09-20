using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DialogFoundation.Backend.LG;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.API
{
    /// <summary>
    /// Service agent that's used to communicate requests/responses with language generation runtime service.
    /// </summary>
    public interface IServiceAgent
    {
        /// <summary>
        /// Generate async is used to generate responses (aka, resolve user referenced templates) using language generation cognitive service.
        /// </summary>
        /// <param name="request">A <see cref="LGRequest"/> object containing the referenced template and slots used from the language generation runtime.</param>
        /// <returns>A <see cref="Task"/> containing the generation result.</returns>
        Task<string> GenerateAsync(LGRequest request);
    }
}
