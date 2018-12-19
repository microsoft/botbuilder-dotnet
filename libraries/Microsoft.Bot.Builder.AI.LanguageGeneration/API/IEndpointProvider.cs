using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.API
{
    /// <summary>
    /// Provides the endpoints required by the resolver to call the language generation service
    /// </summary>
    internal interface IEndpointProvider
    {
        string ResolverEndpoint { get; }

        string TokenGenerationEndpoint { get; }
    }
}
