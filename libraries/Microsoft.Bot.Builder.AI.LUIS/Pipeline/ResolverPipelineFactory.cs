using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.AI.LanguageGeneration.API;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Engine
{
    internal class ResolverPipelineFactory : IResolverPipelineFactory
    {
        public IResolverPipeline CreateResolverPipeline(string endpointURI, IServiceAgent serviceAgent = null)
        {
            var slotBuilder = new SlotBuilder();
            var requestBuilder = new RequestBuilder();
            var responseGenerator = new ResponseGenerator();
            var activityModifier = new ActivityModifier();
            if (serviceAgent != null)
                return new ResolverPipeline(slotBuilder, requestBuilder, responseGenerator, activityModifier, serviceAgent);
            else
                serviceAgent = new ServiceAgent(endpointURI);
            return new ResolverPipeline(slotBuilder, requestBuilder, responseGenerator, activityModifier, serviceAgent);

        }
    }
}
