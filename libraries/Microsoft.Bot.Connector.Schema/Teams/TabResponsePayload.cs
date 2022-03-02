// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema.Teams
{
    /// <summary>
    /// Payload for Tab Response.
    /// </summary>
    public class TabResponsePayload
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TabResponsePayload"/> class.
        /// </summary>
        public TabResponsePayload()
        {
            CustomInit();
        }

        /// <summary>
        /// Gets or sets choice of action options when responding to the
        /// tab/fetch message. Possible values include: 'continue', 'auth' or 'silentAuth'.
        /// </summary>
        /// <value>
        /// One of either: 'continue', 'auth' or 'silentAuth'.
        /// </value>
        [JsonPropertyName("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="TabResponseCards"/> when responding to 
        /// tab/fetch activity with type of 'continue'.
        /// </summary>
        /// <value>
        /// Cards in response to a <see cref="TabResponseCards"/>.
        /// </value>
        [JsonPropertyName("value")]
        public TabResponseCards Value { get; set; }

        /// <summary>
        /// Gets or sets the Suggested Actions for this card tab.
        /// </summary>
        /// <value>
        /// The Suggested Actions for this card tab.
        /// </value>
        [JsonPropertyName("suggestedActions")]
        public TabSuggestedActions SuggestedActions { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        private void CustomInit()
        {
        }
    }
}
