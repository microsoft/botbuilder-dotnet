// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Text;
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
        /// Gets or sets appId for the luis model.
        /// </summary>
        [JsonProperty("appId")]
        public string AppId { get; set; }

        /// <summary>
        /// Gets or sets authoringKey for interacting with service management.
        /// </summary>
        [JsonProperty("authoringKey")]
        public string AuthoringKey { get; set; }

        /// <summary>
        /// Gets or sets subscriptionKey for accessing this service.
        /// </summary>
        [JsonProperty("subscriptionKey")]
        public string SubscriptionKey { get; set; }

        /// <summary>
        /// Gets or sets version of the luis app.
        /// </summary>
        [JsonProperty("version")]
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets region.
        /// </summary>
        [JsonProperty("region")]
        public string Region { get; set; }

        /// <inheritdoc/>
        public override void Encrypt(string secret)
        {
            base.Encrypt(secret);
            this.AuthoringKey = this.AuthoringKey.Encrypt(secret);
            this.SubscriptionKey = this.SubscriptionKey.Encrypt(secret);
        }

        /// <inheritdoc/>
        public override void Decrypt(string secret)
        {
            base.Decrypt(secret);
            this.AuthoringKey = this.AuthoringKey.Decrypt(secret);
            this.SubscriptionKey = this.SubscriptionKey.Decrypt(secret);
        }
    }
}
