using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Engine
{
    /// <summary>
    /// The main language generation resolver pipeline.
    /// </summary>
    internal interface IResolverPipeline
    {
        /// <summary>
        /// The entry point for the pipeline, that executes all the necessary language generation logic.
        /// </summary>
        /// <param name="activity">A <see cref="Activity"/> object.</param>
        /// <param name="entities">A <see cref="IDictionary{string, object}"/> that contains entities/slots used to resolve referenced templates.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task ExecuteAsync(Activity activity, IDictionary<string, object> entities);
    }
}
