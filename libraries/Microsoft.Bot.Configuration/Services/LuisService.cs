// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Configuration
{
    using Microsoft.Bot.Configuration.Encryption;
    using Newtonsoft.Json;

    public class LuisService : ConnectedService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LuisService"/> class.
        /// </summary>
        public LuisService()
            : base(ServiceTypes.Luis)
        {
        }

        /// <summary>
        /// Gets or sets appId for the LUIS model.
        /// </summary>
        /// <value>The App Id.</value>
        [JsonProperty("appId")]
        public string AppId { get; set; }

        /// <summary>
        /// Gets or sets authoringKey for interacting with service management.
        /// </summary>
        /// <value>The Authoring Key.</value>
        [JsonProperty("authoringKey")]
        public string AuthoringKey { get; set; }

        /// <summary>
        /// Gets or sets subscriptionKey for accessing this service.
        /// </summary>
        /// <value>The Subscription Key.</value>
        [JsonProperty("subscriptionKey")]
        public string SubscriptionKey { get; set; }

        /// <summary>
        /// Gets or sets version of the LUIS app.
        /// </summary>
        /// <value>The Version of the LUIS app.</value>
        [JsonProperty("version")]
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets region.
        /// </summary>
        /// <value>The Region.</value>
        [JsonProperty("region")]
        public string Region { get; set; }

        /// <summary>
        /// Gets the endpoint for this LUIS service.
        /// </summary>
        /// <returns>The URL for this service.</returns>
        public string GetEndpoint()
        {
            if (string.IsNullOrWhiteSpace(this.Region))
            {
                throw new System.NullReferenceException("LuisService.Region cannot be Null");
            }

            var region = this.Region.ToLower();

            // usgovvirginia is that actual azure region name, but the cognitive service team called their endpoint 'virginia' instead of 'usgovvirginia'
            // We handle both region names as an alias for virginia.api.cognitive.microsoft.us
            if (region == "virginia" || region == "usgovvirginia")
            {
                return $"https://virginia.api.cognitive.microsoft.us";
            }

            // if it starts with usgov or usdod then it is a .us TLD
            else if (region.StartsWith("usgov") || region.StartsWith("usdod"))
            {
                return $"https://{this.Region}.api.cognitive.microsoft.us";
            }

            return $"https://{this.Region}.api.cognitive.microsoft.com";
        }

        /// <inheritdoc/>
        public override void Encrypt(string secret)
        {
            base.Encrypt(secret);

            if (!string.IsNullOrEmpty(this.AuthoringKey))
            {
                this.AuthoringKey = this.AuthoringKey.Encrypt(secret);
            }

            if (!string.IsNullOrEmpty(this.SubscriptionKey))
            {
                this.SubscriptionKey = this.SubscriptionKey.Encrypt(secret);
            }
        }

        /// <inheritdoc/>
        public override void Decrypt(string secret)
        {
            base.Decrypt(secret);

            if (!string.IsNullOrEmpty(this.AuthoringKey))
            {
                this.AuthoringKey = this.AuthoringKey.Decrypt(secret);
            }

            if (!string.IsNullOrEmpty(this.SubscriptionKey))
            {
                this.SubscriptionKey = this.SubscriptionKey.Decrypt(secret);
            }
        }
    }
}
