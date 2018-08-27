using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Ai.LanguageGeneration.API;

namespace Microsoft.Bot.Builder.Ai.LanguageGeneration.Engine
{
    internal class ResolverPipelineFactory : IResolverPipelineFactory
    {
        public IResolverPipeline CreateResolverPipeline(string endpointURI)
        {
            var slotBuilder = new SlotBuilder();
            var requestBuilder = new RequestBuilder();
            var responseGenerator = new ResponseGenerator();
            var activityModifier = new ActivityModifier();
            var serviceAgent = new ServiceAgent(endpointURI);

            return new ResolverPipeline(slotBuilder, requestBuilder, responseGenerator, activityModifier, serviceAgent);
        }
    }
}
