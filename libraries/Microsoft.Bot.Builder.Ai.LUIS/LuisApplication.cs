using System;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;

namespace Microsoft.Bot.Builder.Ai.Luis
{
    /// <summary>
    /// Data describing a LUIS application.
    /// </summary>
    public class LuisApplication
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LuisApplication"/> class.
        /// </summary>
        /// <param name="applicationId">LUIS application ID.</param>
        /// <param name="subscriptionKey">LUIS subscription or endpoint key.</param>
        /// <param name="azureRegion">Azure region with endpoint.</param>
        public LuisApplication(string applicationId, string subscriptionKey, string azureRegion)
        {
            if (!Guid.TryParse(applicationId, out var appGuid))
            {
                throw new ArgumentException($"\"{applicationId}\" is not a valid LUIS application id.");
            }

            if (!Guid.TryParse(subscriptionKey, out var subscriptionGuid))
            {
                throw new ArgumentException($"\"{applicationId}\" is not a valid LUIS subscription key.");
            }

            if (!Enum.TryParse<AzureRegions>(azureRegion, out var region))
            {
                throw new ArgumentException($"\"{azureRegion}\" is not a valid LUIS region.");
            }

            ApplicationId = applicationId;
            SubscriptionKey = subscriptionKey;
            AzureRegion = azureRegion;
        }

        /// <summary>
        /// Gets or sets LUIS application ID.
        /// </summary>
        /// <value>
        /// LUIS application ID.
        /// </value>
        public string ApplicationId { get; set; }

        /// <summary>
        /// Gets or sets LUIS subscription or endpoint key.
        /// </summary>
        /// <value>
        /// LUIS subscription or endpoint key.
        /// </value>
        public string SubscriptionKey { get; set; }

        /// <summary>
        /// Gets or sets Azure Region where endpoint is located.
        /// </summary>
        /// <value>
        /// Azure Region where endpoint is located.
        /// </value>
        public string AzureRegion { get; set; }
    }
}
