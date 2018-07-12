using System;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime;

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
        /// <param name="appId">LUIS AppId.</param>
        /// <param name="subscriptionKey">LUIS subscription or endpoint key.</param>
        /// <param name="azureRegion">Azure region with endpoint.</param>
        public LuisApplication(string appId, string subscriptionKey, string azureRegion)
        {
            if (string.IsNullOrEmpty(appId))
            {
                throw new ArgumentNullException(nameof(appId));
            }

            if (string.IsNullOrEmpty(subscriptionKey))
            {
                throw new ArgumentNullException(nameof(subscriptionKey));
            }

            ApplicationId = appId;
            SubscriptionKey = subscriptionKey;
            AzureRegion = azureRegion;
        }

        /// <summary>
        /// Gets or sets LUIS application ID.
        /// </summary>
        public string ApplicationId { get; set; }

        /// <summary>
        /// Gets or sets LUIS subscription or endpoint key.
        /// </summary>
        public string SubscriptionKey { get; set; }

        /// <summary>
        /// Gets or sets Azure Region where endpoint is located.
        /// </summary>
        public string AzureRegion { get; set; }
    }
}
