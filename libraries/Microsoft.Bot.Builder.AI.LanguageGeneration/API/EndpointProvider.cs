using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.AI.LanguageGeneration.Resolver;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.API
{
    internal class EndpointProvider : IEndpointProvider
    {
        public EndpointProvider(LanguageGenerationApplication languageGenerationApplication, LanguageGenerationOptions languageGenerationOptions)
        {
            if (languageGenerationApplication == null)
            {
                throw new ArgumentNullException(nameof(languageGenerationApplication));
            }

            if (languageGenerationOptions == null)
            {
                throw new ArgumentNullException(nameof(languageGenerationOptions));
            }

            var defaultEndpoint = new DefaultRegionalEndpointProvider(languageGenerationApplication.ApplicationRegion);

            ResolverEndpoint = defaultEndpoint.ResolverEndpoint;
            if (!string.IsNullOrEmpty(languageGenerationOptions.ResolverApiEndpoint))
            {
                ResolverEndpoint = languageGenerationOptions.ResolverApiEndpoint;
            }

            TokenGenerationEndpoint = defaultEndpoint.TokenGenerationEndpoint;
            if (!string.IsNullOrEmpty(languageGenerationOptions.TokenGenerationApiEndpoint))
            {
                TokenGenerationEndpoint = languageGenerationOptions.TokenGenerationApiEndpoint;
            }
        }

        public string ResolverEndpoint { get; private set; }

        public string TokenGenerationEndpoint { get; private set; }
    }
}
