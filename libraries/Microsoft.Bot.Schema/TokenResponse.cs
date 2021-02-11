// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Response schema sent back from Bot Framework Token Service, in response to a request to get or exchange a token for a user.
    /// </summary>
    public partial class TokenResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TokenResponse"/> class.
        /// </summary>
        public TokenResponse()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenResponse"/> class.
        /// </summary>
        /// <param name="channelId">The channel ID.</param>
        /// <param name="connectionName">The connection name.</param>
        /// <param name="token">The token.</param>
        /// <param name="expiration">The expiration.</param>
        public TokenResponse(string channelId = default(string), string connectionName = default(string), string token = default(string), string expiration = default(string))
        {
            ChannelId = channelId;
            ConnectionName = connectionName;
            Token = token;
            Expiration = expiration;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets the channel ID.
        /// </summary>
        /// <value>The channel ID.</value>
        [JsonProperty(PropertyName = "channelId")]
        public string ChannelId { get; set; }

        /// <summary>
        /// Gets or sets the connection name.
        /// </summary>
        /// <value>The connection name.</value>
        [JsonProperty(PropertyName = "connectionName")]
        public string ConnectionName { get; set; }

        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        /// <value>The token.</value>
        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the expiration.
        /// </summary>
        /// <value>The expiration.</value>
        [JsonProperty(PropertyName = "expiration")]
        public string Expiration { get; set; }
        
        /// <summary>
        /// Gets or sets extra propreties.
        /// </summary>
        /// <value>The extra properties.</value>
        [JsonExtensionData(ReadData = true, WriteData = true)]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking compat).
        public JObject Properties { get; set; } = new JObject();
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
