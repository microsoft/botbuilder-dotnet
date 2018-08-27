using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Ai.LanguageGeneration.Resolver
{
    public class LGEndpoint
    {
        public LGEndpoint(string endpointKey, string lgAppId, string endpointUri)
        {
            ValidateParameters(endpointKey, lgAppId, endpointUri);
            EndpointKey = endpointKey;
            LGAppId = lgAppId;
            EndpointURI = endpointUri;
        }

        private void ValidateParameters(string endpointKey, string lgAppId, string endpointUri)
        {
            if (string.IsNullOrWhiteSpace(endpointKey))
            {
                throw new ArgumentNullException(nameof(endpointKey));
            }

            if (string.IsNullOrWhiteSpace(lgAppId))
            {
                throw new ArgumentNullException(nameof(lgAppId));
            }

            if (string.IsNullOrWhiteSpace(endpointUri))
            {
                throw new ArgumentNullException(nameof(endpointUri));
            }
        }
        public string EndpointKey { get; private set; }
        public string LGAppId { get; private set; }
        public string EndpointURI { get; private set; }
    }
}
