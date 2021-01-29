// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

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
        /// Gets or sets the details of why the token exchange failed.
        /// </summary>
        /// <value>
        /// The details of why the token exchange failed.
        /// </value>
        [JsonProperty("failureDetail")]
        public string FailureDetail { get; set; }

        /// <summary>
        /// Gets or sets extension data for overflow of properties.
        /// </summary>
        /// <value>
        /// Extension data for overflow of properties.
        /// </value>
        [JsonExtensionData(ReadData = true, WriteData = true)]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        public JObject Properties { get; set; } = new JObject();
#pragma warning restore CA2227 // Collection properties should be read only
    }
}
