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
    internal class ResolverPipelineFactory : IResolverPipelineFactory
    {
        /// <summary>
        /// Creates a new <see cref="IResolverPipeline"/> object using the passed arguments.
        /// </summary>
        /// <param name="languageGenerationApplication">Language Generation Application identifing parameters</param>
        /// <param name="endpointProvider">An provider object to provide the endpoints required during resolution and authentication token generation</param>
        /// <returns>A <see cref="IResolverPipeline"/>.</returns>
        public IResolverPipeline CreateResolverPipeline(LanguageGenerationApplication languageGenerationApplication, IEndpointProvider endpointProvider)
        {
            if (languageGenerationApplication == null)
            {
                throw new ArgumentNullException(nameof(languageGenerationApplication));
            }

            var serviceAgent = new ServiceAgent(endpointProvider.ResolverEndpoint, languageGenerationApplication.SubscriptionKey, endpointProvider.TokenGenerationEndpoint);

            return CreateResolverPipeline(languageGenerationApplication, serviceAgent);
        }

        /// <summary>
        /// Creates a new <see cref="IResolverPipeline"/> object using the passed arguments.
        /// </summary>
        /// <param name="languageGenerationApplication">Language Generation Application identifing parameters</param>
        /// <param name="serviceAgent">A <see cref="IServiceAgent"/> object.</param>
        /// <returns>A <see cref="IResolverPipeline"/>.</returns>
        public IResolverPipeline CreateResolverPipeline(LanguageGenerationApplication languageGenerationApplication, IServiceAgent serviceAgent)
        { 
            if (languageGenerationApplication == null)
            {
                throw new ArgumentNullException(nameof(languageGenerationApplication));
            }

            if (serviceAgent == null)
            {
                throw new ArgumentNullException(nameof(serviceAgent));
            }

            var slotBuilder = new SlotBuilder();
            var requestBuilder = new RequestBuilder(languageGenerationApplication.ApplicationId, languageGenerationApplication.ApplicationLocale, languageGenerationApplication.ApplicationVersion);
            var responseGenerator = new ResponseGenerator();
            var activityModifier = new ActivityModifier();

            return new ResolverPipeline(slotBuilder, requestBuilder, responseGenerator, activityModifier, serviceAgent);
        }
    }
}
