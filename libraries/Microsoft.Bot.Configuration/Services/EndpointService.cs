// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Configuration
{
    using Microsoft.Bot.Configuration.Encryption;
    using Newtonsoft.Json;

    public class EndpointService : ConnectedService
    {
        public EndpointService()
            : base(ServiceTypes.Endpoint)
        {
        }

        /// <summary>
        /// Gets or sets appId for the bot.
        /// </summary>
        [JsonProperty("appId")]
        public string AppId { get; set; }

        /// <summary>
        /// Gets or sets app password for the bot.
        /// </summary>
        [JsonProperty("appPassword")]
        public string AppPassword { get; set; }

        /// <summary>
        /// Gets or sets the channel service (Azure or US Government Azure) for the bot.
        /// </summary>
        [JsonProperty("channelService")]
        public string ChannelService { get; set; }

        /// <summary>
        /// Gets or sets endpoint url for the bot.
        /// </summary>
        [JsonProperty("endpoint")]
        public string Endpoint { get; set; }

        /// <inheritdoc/>
        public override void Encrypt(string secret)
        {
            base.Encrypt(secret);

            if (!string.IsNullOrEmpty(this.AppPassword))
            {
                this.AppPassword = this.AppPassword.Encrypt(secret);
            }
        }

        /// <inheritdoc/>
        public override void Decrypt(string secret)
        {
            base.Decrypt(secret);

            if (!string.IsNullOrEmpty(this.AppPassword))
            {
                this.AppPassword = this.AppPassword.Decrypt(secret);
            }
        }
    }
}
