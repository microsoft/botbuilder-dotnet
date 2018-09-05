// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// The status of a particular token
    /// </summary>
    public class TokenStatus
    {
        /// <summary>
        /// The name of the connection the token status pertains to
        /// </summary>
        [JsonProperty("connectionName")]
        public string ConnectionName { get; set; }

        /// <summary>
        /// Whether there is a token or not
        /// </summary>
        [JsonProperty("hasToken")]
        public bool HasToken { get; set; }

        /// <summary>
        /// The display name of the service provider for which this Token belongs to
        /// </summary>
        [JsonProperty("serviceProviderDisplayName")]
        public string ServiceProviderDisplayName { get; set; }
    }
}
