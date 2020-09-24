// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Configuration
{
    using System;
    using Microsoft.Bot.Configuration.Encryption;
    using Newtonsoft.Json;

    /// <summary>
    /// An Endpoint service containing configuration properties defining the endpoint for a bot (Azure or Government).
    /// </summary>
    [Obsolete("This class is deprecated.  See https://aka.ms/bot-file-basics for more information.", false)]
    public class EndpointService : ConnectedService
    { 
        /// <summary>
        /// Initializes a new instance of the <see cref="EndpointService"/> class.
        /// </summary>
        public EndpointService()
            : base(ServiceTypes.Endpoint)
        {
        }

        /// <summary>
        /// Gets or sets appId for the bot.
        /// </summary>
        /// <value>The App Id.</value>
        [JsonProperty("appId")]
        public string AppId { get; set; }

        /// <summary>
        /// Gets or sets app password for the bot.
        /// </summary>
        /// <value>The App password.</value>
        [JsonProperty("appPassword")]
        public string AppPassword { get; set; }

        /// <summary>
        /// Gets or sets the channel service (Azure or US Government Azure) for the bot.
        /// </summary>
        /// <value>The Channel Service.</value>
        [JsonProperty("channelService")]
        public string ChannelService { get; set; }

        /// <summary>
        /// Gets or sets endpoint url for the bot.
        /// </summary>
        /// <value>The Endpoint for the Bot.</value>
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
