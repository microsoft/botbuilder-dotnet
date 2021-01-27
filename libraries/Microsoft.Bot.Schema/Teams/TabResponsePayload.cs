// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;
    
    /// <summary>
    /// Payload for Tab Response.
    /// </summary>
    public partial class TabResponsePayload
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
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="TabResponseCards"/> when responding to 
        /// tab/fetch activity with type of 'continue'.
        /// </summary>
        /// <value>
        /// Cards in response to a <see cref="TabResponseCards"/>.
        /// </value>
        [JsonProperty(PropertyName = "value")]
        public TabResponseCards Value { get; set; }

        /// <summary>
        /// Gets or sets the Suggested Actions for this card tab.
        /// </summary>
        /// <value>
        /// The Suggested Actions for this card tab.
        /// </value>
        [JsonProperty(PropertyName = "suggestedActions")]
        public TabSuggestedActions SuggestedActions { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
