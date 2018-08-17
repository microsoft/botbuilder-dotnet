using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Ai.LanguageGeneration.Engine
{
    internal interface IResolverPipelineFactory
    {
        IResolverPipeline CreateResolverPipeline();
    }
}
