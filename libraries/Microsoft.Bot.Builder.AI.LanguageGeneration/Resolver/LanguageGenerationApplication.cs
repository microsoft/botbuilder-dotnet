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
        public LanguageGenerationApplication(string applicationId, string applicationRegion, string applicationLocale, string applicationVersion, string subscriptionKey)
        {
            if (string.IsNullOrWhiteSpace(applicationId))
            {
                throw new ArgumentException($"\"{applicationId}\" is not a valid Language generation application id.", nameof(applicationId));
            }

            if (string.IsNullOrWhiteSpace(applicationRegion))
            {
                throw new ArgumentException($"\"{applicationRegion}\" is not a valid region.", nameof(applicationRegion));
            }

            if (string.IsNullOrWhiteSpace(applicationLocale))
            {
                throw new ArgumentException($"\"{applicationLocale}\" is not a valid locale.", nameof(applicationLocale));
            }

            if (string.IsNullOrWhiteSpace(applicationVersion))
            {
                throw new ArgumentException($"\"{applicationVersion}\" is not a valid application version.", nameof(applicationVersion));
            }

            if (!Guid.TryParse(subscriptionKey, out var subscriptionGuid))
            {
                throw new ArgumentException($"\"{subscriptionKey}\" is not a valid Language generation subscription key.", nameof(subscriptionKey));
            }

            ApplicationId = applicationId;
            ApplicationRegion = applicationRegion;
            ApplicationLocale = applicationLocale;
            ApplicationVersion = applicationVersion;
            SubscriptionKey = subscriptionKey;
        }

        /// <summary>
        /// Gets or sets language generation application id.
        /// </summary>
        /// <value>
        /// LUIS _application ID.
        /// </value>
        public string ApplicationId { get; private set; }

        /// <summary>
        /// Gets or sets language generation application region.
        /// </summary>
        /// <value>
        /// LUIS application region.
        /// </value>
        public string ApplicationRegion { get; private set; }


        /// <summary>
        /// Gets or sets language generation application locale.
        /// </summary>
        /// <value>
        /// LUIS application locale.
        /// </value>
        public string ApplicationLocale { get; private set; }

        /// <summary>
        /// Gets or sets language generation application version.
        /// </summary>
        /// <value>
        /// LUIS application version.
        /// </value>
        public string ApplicationVersion { get; private set; }

        /// <summary>
        /// Gets or sets language generation subscription key.
        /// </summary>
        /// <value>
        /// LUIS subscription key.
        /// </value>
        public string SubscriptionKey { get; private set; }
    }
}
