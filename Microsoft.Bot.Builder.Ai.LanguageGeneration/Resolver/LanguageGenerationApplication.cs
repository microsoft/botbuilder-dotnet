using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Resolver
{
    public class LanguageGenerationApplication
    {
        public LanguageGenerationApplication(string applicationId, string endpointKey, string endpointUri)
        {
            if (string.IsNullOrWhiteSpace(applicationId))
            {
                throw new ArgumentException($"\"{applicationId}\" is not a valid Language generation application id.");
            }

            if (!Guid.TryParse(endpointKey, out var subscriptionGuid))
            {
                throw new ArgumentException($"\"{endpointKey}\" is not a valid Language generation subscription key.");
            }

            if (string.IsNullOrWhiteSpace(endpointUri))
            {
                throw new ArgumentException($"\"{endpointUri}\" is not a valid Language generation endpoint.");
            }

            ApplicationId = applicationId;
            EndpointKey = endpointKey;
            EndpointUri = endpointUri;
        }

        /// <summary>
        /// Gets or sets LUIS _application ID.
        /// </summary>
        /// <value>
        /// LUIS _application ID.
        /// </value>
        public string ApplicationId { get; set; }

        /// <summary>
        /// Gets or sets LUIS subscription or endpoint key.
        /// </summary>
        /// <value>
        /// LUIS subscription or endpoint key.
        /// </value>
        public string EndpointKey { get; private set; }

        /// <summary>
        /// Gets or sets Azure Region where endpoint is located.
        /// </summary>
        /// <value>
        /// Azure Region where endpoint is located.
        /// </value>
        public string EndpointUri { get; set; }
    }
}
