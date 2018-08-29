using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Resolver
{
    public class LanguageGenerationApplication
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum AzureRegions
        {
            Westus = 0,
            Westeurope = 1,
            Southeastasia = 2,
            Eastus2 = 3,
            Westcentralus = 4,
            Westus2 = 5,
            Eastus = 6,
            Southcentralus = 7,
            Northeurope = 8,
            Eastasia = 9,
            Australiaeast = 10,
            Brazilsouth = 11
        }

        public LanguageGenerationApplication(string applicationId, string endpointKey, string azureRegion)
        {
            if (!Guid.TryParse(applicationId, out var appGuid))
            {
                throw new ArgumentException($"\"{applicationId}\" is not a valid Language generation application id.");
            }

            if (!Guid.TryParse(endpointKey, out var subscriptionGuid))
            {
                throw new ArgumentException($"\"{applicationId}\" is not a valid Language generation subscription key.");
            }

            if (azureRegion != null && azureRegion.Length > 0)
            {
                // Enum values are normalized to first char being capital and the rest lower
                azureRegion = char.ToUpper(azureRegion[0]) + azureRegion.Substring(1).ToLower();
            }

            if (!Enum.TryParse<AzureRegions>(azureRegion, out var region))
            {
                throw new ArgumentException($"\"{azureRegion}\" is not a valid LUIS region.");
            }

            ApplicationId = applicationId;
            EndpointKey = endpointKey;
            AzureRegion = azureRegion;
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
        public string AzureRegion { get; set; }
    }
}
