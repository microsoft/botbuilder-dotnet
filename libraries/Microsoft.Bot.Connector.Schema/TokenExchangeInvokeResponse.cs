// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema
{
    /// <summary>
    /// The response object of a token exchange invoke.
    /// </summary>
    public class TokenExchangeInvokeResponse
    {
        /// <summary>
        /// Gets or sets the id from the TokenExchangeInvokeRequest.
        /// </summary>
        /// <value>
        /// The id from the TokenExchangeInvokeRequest.
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
        /// Gets or sets the details of why the token exchange failed.
        /// </summary>
        /// <value>
        /// The details of why the token exchange failed.
        /// </value>
        [JsonPropertyName("failureDetail")]
        public string FailureDetail { get; set; }

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
