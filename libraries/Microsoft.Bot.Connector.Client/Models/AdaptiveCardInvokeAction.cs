// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Client.Models
{
    /// <summary>
    /// Defines the structure that arrives in the Activity.Value.Action for Invoke activity with Name of 'adaptiveCard/action'.
    /// </summary>
    public class AdaptiveCardInvokeAction
    {
        /// <summary>
        /// Gets or sets the Type of this adaptive card action invoke.
        /// </summary>
        /// <value>
        /// The Type of this Adaptive Card Invoke Action.
        /// </value>
        [JsonPropertyName("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the Id of this adaptive card action invoke.
        /// </summary>
        /// <value>
        /// The Id of this Adaptive Card Invoke Action.
        /// </value>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the Verb of this adaptive card action invoke.
        /// </summary>
        /// <value>
        /// The Verb of this adaptive card action invoke.
        /// </value>
        [JsonPropertyName("verb")]
        public string Verb { get; set; }

        /// <summary>
        /// Gets or sets the Data of this adaptive card action invoke.
        /// </summary>
        /// <value>
        /// The Data of this adaptive card action invoke.
        /// </value>
        [JsonPropertyName("data")]
        public object Data { get; set; }
    }
}
