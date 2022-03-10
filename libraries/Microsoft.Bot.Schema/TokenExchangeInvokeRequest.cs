// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// A request to exchange a token.
    /// </summary>
    public class TokenExchangeInvokeRequest
    {
        /// <summary>
        /// Gets or sets the id from the OAuthCard.
        /// </summary>
        /// <value>
        /// The id from the OAuthCard.
        /// </value>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the connection name.
        /// </summary>
        /// <value>
        /// The connection name.
        /// </value>
        [JsonProperty("connectionName")]
        public string ConnectionName { get; set; }

        /// <summary>
        /// Gets or sets the user token that can be exchanged.
        /// </summary>
        /// <value>
        /// The user token that can be exchanged.
        /// </value>
        [JsonProperty("token")]
        public string Token { get; set; }

        /// <summary>
        /// Gets extension data for overflow of properties.
        /// </summary>
        /// <value>
        /// Extension data for overflow of properties.
        /// </value>
        [JsonExtensionData(ReadData = true, WriteData = true)]
        public JObject Properties { get; private set; } = new JObject();
    }
}
