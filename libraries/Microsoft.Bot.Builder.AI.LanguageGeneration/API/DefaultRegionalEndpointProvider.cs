using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.API
{
    internal class DefaultRegionalEndpointProvider : IEndpointProvider
    {
        private const string ResolverEndpointPattern = "https://{0}.cts.speech.microsoft.com/v1/lg";
        private const string TokenGenerationEndpointPattern = "https://{0}.api.cognitive.microsoft.com/sts/v1.0/issueToken";

        public DefaultRegionalEndpointProvider(string region)
        {
            if (string.IsNullOrWhiteSpace(region))
            {
                throw new ArgumentException("Value shouldn't be null or empty.", nameof(region));
            }

            ResolverEndpoint = string.Format(ResolverEndpointPattern, region);
            TokenGenerationEndpoint = string.Format(TokenGenerationEndpointPattern, region);
        }

        public string ResolverEndpoint { get; private set; }

        public string TokenGenerationEndpoint { get; private set; }
    }
}
