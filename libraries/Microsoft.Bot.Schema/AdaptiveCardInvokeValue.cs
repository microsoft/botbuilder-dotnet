// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Schema
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
        [JsonProperty("action")]
        public AdaptiveCardInvokeAction Action { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="AdaptiveCardAuthentication"/> for this adaptive card invoke action value.
        /// </summary>
        /// <value>
        /// The <see cref="AdaptiveCardAuthentication"/> for this adaptive card invoke action value.
        /// </value>
        [JsonProperty("authentication")]
        public AdaptiveCardAuthentication Authentication { get; set; }

        /// <summary>
        /// Gets or sets the 'state' or magic code for an OAuth flow.
        /// </summary>
        /// <value>
        /// The 'state' or magic code for an OAuth flow.
        /// </value>
        [JsonProperty("state")]
        public string State { get; set; }
    }
}
