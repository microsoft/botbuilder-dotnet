using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.AI.LanguageGeneration.API;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Engine
{
    internal interface IResolverPipelineFactory
    {
        IResolverPipeline CreateResolverPipeline(string endpointURI, string endpointKey, string applicationId, string tokenGenerationEndpoint = null, IServiceAgent serviceAgent = null);
    }
}
