// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Defines the structure that arrives in the Activity.Value.Authentication for Invoke activity with Name of 'adaptiveCard/action'.
    /// </summary>
    public class AdaptiveCardAuthentication
    {
        /// <summary>
        /// Gets or sets the Id of the adaptive card invoke authentication.
        /// </summary>
        /// <value>
        /// The id of this adaptive card invoke action value's 'authentication'.
        /// </value>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the connection name of the adaptive card authentication.
        /// </summary>
        /// <value>
        /// The connection name of the adaptive card authentication.
        /// </value>
        [JsonProperty("connectionName")]
        public string ConnectionName { get; set; }

        /// <summary>
        /// Gets or sets the token of the adaptive card authentication.
        /// </summary>
        /// <value>
        /// The token of the adaptive card authentication.
        /// </value>
        [JsonProperty("token")]
        public string Token { get; set; }
    }
}
