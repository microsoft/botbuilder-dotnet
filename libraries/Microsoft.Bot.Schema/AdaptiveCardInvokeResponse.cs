// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Schema
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
        [JsonProperty("statusCode")]
        public int StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the Type of this <see cref="AdaptiveCardInvokeResponse"/>.
        /// </summary>
        /// <value>
        /// The Type of this response.
        /// </value>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the json response object.
        /// </summary>
        /// <value>
        /// The json response object.
        /// </value>
        [JsonProperty("value")]
        public object Value { get; set; }
    }
}
