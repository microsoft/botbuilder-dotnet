// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema
{
    /// <summary>
    /// Helps provide polling for token details.
    /// </summary>
    public class TokenPollingSettings
    {
        /// <summary>
        /// Gets or sets polling timeout time in milliseconds. This is equivalent to login flow timeout.
        /// </summary>
        /// <value>
        /// Login timeout value.
        /// </value>
        [JsonPropertyName("timeout")]
        public int Timeout { get; set; }

        /// <summary>
        /// Gets or sets time Interval in milliseconds between token polling requests.
        /// </summary>
        /// /// <value>
        /// Time interval between successive requests.
        /// </value>
        [JsonPropertyName("interval")]
        public int Interval { get; set; }
    }
}
