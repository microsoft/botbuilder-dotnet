using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.AI.LanguageGeneration.API;

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
        /// <param name="endpoint">Language generation runtime api endpoint.</param>
        /// <param name="endpointKey">Language generation runtime api endpoint key.</param>
        /// <param name="applicationId">Language generation application id.</param>
        /// <param name="tokenGenerationEndpoint">Token generation endpoint, default is "https://api.cognitive.microsoft.com/sts/v1.0/issueToken".</param>
        /// <param name="serviceAgent">A <see cref="IServiceAgent"/> object.</param>
        /// <returns>A <see cref="IResolverPipeline"/>.</returns>
        public IResolverPipeline CreateResolverPipeline(string endpointURI, string endpointKey, string applicationId, string tokenGenerationEndpoint = null, IServiceAgent serviceAgent = null)
        {
            var slotBuilder = new SlotBuilder();
            var localeExtractor = new LocaleExtractor();
            var requestBuilder = new RequestBuilder(applicationId);
            var responseGenerator = new ResponseGenerator();
            var activityModifier = new ActivityModifier();
            if (serviceAgent != null)
                return new ResolverPipeline(slotBuilder, localeExtractor, requestBuilder, responseGenerator, activityModifier, serviceAgent);
            else
                serviceAgent = new ServiceAgent(endpointURI, endpointKey, tokenGenerationEndpoint);
            return new ResolverPipeline(slotBuilder, localeExtractor, requestBuilder, responseGenerator, activityModifier, serviceAgent);
        }
    }
}
