// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Client.Models
{
    /// <summary>
    /// Defines the structure that is returned as the result of an Invoke activity with Name of 'adaptiveCard/action'.
    /// </summary>
    public class AdaptiveCardInvokeResponse
    {
        /// <summary>
        /// Gets or sets the Card Action response StatusCode.
        /// </summary>
        /// <value>
        /// The Card Action response StatusCode.
        /// </value>
        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the Type of this <see cref="AdaptiveCardInvokeResponse"/>.
        /// </summary>
        /// <value>
        /// The Type of this response.
        /// </value>
        [JsonPropertyName("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the json response object.
        /// </summary>
        /// <value>
        /// The json response object.
        /// </value>
        [JsonPropertyName("value")]
        public object Value { get; set; }
    }
}
