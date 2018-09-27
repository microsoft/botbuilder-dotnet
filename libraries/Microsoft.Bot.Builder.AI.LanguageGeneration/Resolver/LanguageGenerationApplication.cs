using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Resolver
{
    /// <summary>
    /// Data describing a language generation application.
    /// </summary>
    public class LanguageGenerationApplication
    {
        public LanguageGenerationApplication(string applicationId, string endpointKey, string endpoint)
        {
            if (string.IsNullOrWhiteSpace(applicationId))
            {
                throw new ArgumentException($"\"{applicationId}\" is not a valid Language generation application id.");
            }

            if (!Guid.TryParse(endpointKey, out var subscriptionGuid))
            {
                throw new ArgumentException($"\"{endpointKey}\" is not a valid Language generation subscription key.");
            }

            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new ArgumentException($"\"{endpoint}\" is not a valid Language generation endpoint.");
            }

            if (!Uri.IsWellFormedUriString(endpoint, UriKind.Absolute))
            {
                throw new ArgumentException($"\"{endpoint}\" is not a valid Language generation endpoint.");
            }

            ApplicationId = applicationId;
            EndpointKey = endpointKey;
            Endpoint = endpoint;
        }

        /// <summary>
        /// Gets or sets language generation application id.
        /// </summary>
        /// <value>
        /// LUIS _application ID.
        /// </value>
        public string ApplicationId { get; set; }

        /// <summary>
        /// Gets or sets language generation subscription or endpoint key.
        /// </summary>
        /// <value>
        /// LUIS subscription or endpoint key.
        /// </value>
        public string EndpointKey { get; private set; }

        /// <summary>
        /// Gets or sets language generation  endpoint where the application is hosted
        /// </summary>
        public string Endpoint { get; set; }
    }
}
