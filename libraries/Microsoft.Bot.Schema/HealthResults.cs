// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Defines the structure that is returned as the result of a health check on the bot.
    /// The health check is sent to the bot as an InvokeActivity and this class along with <see cref="HealthCheckResponse"/> defines the structure of the body of the response.
    /// </summary>
    public class HealthResults
    {
        /// <summary>
        /// Gets or sets a value indicating whether the health check has succeeded and the bot is healthy.
        /// </summary>
        /// <value>A boolean value indicating health status.</value>
        [JsonProperty(PropertyName = "success")]
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets a value that is exactly the same as the Authorization header that would have been added to an HTTP POST back.
        /// </summary>
        /// <value>An Authorization header value.</value>
        [JsonProperty(PropertyName = "authorization")]
        public string Authorization { get; set; }

        /// <summary>
        /// Gets or sets a value that is exactly the same as the User-Agent header that would have been added to an HTTP POST back.
        /// </summary>
        /// <value>A User-Agent header value.</value>
        [JsonProperty(PropertyName = "user-agent")]
        public string UserAgent { get; set; }

        /// <summary>
        /// Gets or sets informational messages that can be optionally included in the health check response.
        /// </summary>
        /// <value>An array of informational message strings.</value>
        [JsonProperty(PropertyName = "messages")]
#pragma warning disable CA1819 // Properties should not return arrays (we can't change this without breaking binary compat).
        public string[] Messages { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays

        /// <summary>
        /// Gets or sets diagnostic data that can be optionally included in the health check response. 
        /// </summary>
        /// <value>Arbitrary diagnostic data that will be serialized to JSON.</value>
        [JsonProperty(PropertyName = "diagnostics")]
        public object Diagnostics { get; set; }
    }
}
