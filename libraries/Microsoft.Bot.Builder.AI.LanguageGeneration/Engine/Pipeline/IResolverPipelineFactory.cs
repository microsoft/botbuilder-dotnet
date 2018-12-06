using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.AI.LanguageGeneration.API;
using Microsoft.Bot.Builder.AI.LanguageGeneration.Resolver;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Engine
{
    /// <summary>
    /// Resolver pipeline factory used to create a resolver pipeline.
    /// </summary>
    internal interface IResolverPipelineFactory
    {
        /// <summary>
        /// Creates a new <see cref="IResolverPipeline"/> object using the passed arguments.
        /// </summary>
        /// <param name="languageGenerationApplication">Language generation application.</param>
        /// <param name="endpointProvider">Resolver and token generation provider.</param>
        /// <returns>A <see cref="IResolverPipeline"/>.</returns>
        IResolverPipeline CreateResolverPipeline(LanguageGenerationApplication languageGenerationApplication, IEndpointProvider endpointProvider);

        /// <summary>
        /// Creates a new <see cref="IResolverPipeline"/> object using the passed arguments.
        /// </summary>
        /// <param name="languageGenerationApplication">Language generation application.</param>
        /// <param name="serviceAgent">A <see cref="IServiceAgent"/> object.</param>
        /// <returns>A <see cref="IResolverPipeline"/>.</returns>
        IResolverPipeline CreateResolverPipeline(LanguageGenerationApplication languageGenerationApplication, IServiceAgent serviceAgent);
    }
}
