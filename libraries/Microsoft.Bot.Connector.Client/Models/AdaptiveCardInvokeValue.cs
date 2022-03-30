// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Client.Models
{
    /// <summary>
    /// Defines the structure that arrives in the Activity.Value for Invoke activity with Name of 'adaptiveCard/action'.
    /// </summary>
    public class AdaptiveCardInvokeValue
    {
        /// <summary>
        /// Gets or sets the action of this adaptive card action invoke value.
        /// </summary>
        /// <value>
        /// The action of this adaptive card invoke action value.
        /// </value>
        [JsonPropertyName("action")]
        public AdaptiveCardInvokeAction Action { get; set; }

        /// <summary>
        /// Gets or sets the 'state' or magic code for an OAuth flow.
        /// </summary>
        /// <value>
        /// The 'state' or magic code for an OAuth flow.
        /// </value>
        [JsonPropertyName("state")]
        public string State { get; set; }
    }
}
