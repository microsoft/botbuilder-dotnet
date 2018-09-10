using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.AI.LanguageGeneration.API;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Engine
{
    internal class ResolverPipelineFactory : IResolverPipelineFactory
    {
        public IResolverPipeline CreateResolverPipeline(string endpointURI, string endpointKey, string applicationId, IServiceAgent serviceAgent = null)
        {
            var slotBuilder = new SlotBuilder();
            var localeExtractor = new LocaleExtractor();
            var requestBuilder = new RequestBuilder(applicationId);
            var responseGenerator = new ResponseGenerator();
            var activityModifier = new ActivityModifier();
            if (serviceAgent != null)
                return new ResolverPipeline(slotBuilder, localeExtractor, requestBuilder, responseGenerator, activityModifier, serviceAgent);
            else
                serviceAgent = new ServiceAgent(endpointURI, endpointKey);
            return new ResolverPipeline(slotBuilder, localeExtractor, requestBuilder, responseGenerator, activityModifier, serviceAgent);
        }
    }
}
