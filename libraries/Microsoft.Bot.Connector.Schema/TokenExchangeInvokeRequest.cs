// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema
{
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
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the connection name.
        /// </summary>
        /// <value>
        /// The connection name.
        /// </value>
        [JsonPropertyName("connectionName")]
        public string ConnectionName { get; set; }

        /// <summary>
        /// Gets or sets the user token that can be exchanged.
        /// </summary>
        /// <value>
        /// The user token that can be exchanged.
        /// </value>
        [JsonPropertyName("token")]
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets extension data for overflow of properties.
        /// </summary>
        /// <value>
        /// Extension data for overflow of properties.
        /// </value>
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Property setter is required for the collection to be deserialized")]
        [JsonExtensionData]
        public Dictionary<string, JsonElement> Properties { get; set; } = new Dictionary<string, JsonElement>();
    }
}
