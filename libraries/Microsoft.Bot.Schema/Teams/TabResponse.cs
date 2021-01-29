// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;
    
    /// <summary>
    /// Envelope for Card Tab Response Payload.
    /// </summary>
    public partial class TabResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TabResponse"/> class.
        /// </summary>
        public TabResponse()
        {
            CustomInit();
        }

        /// <summary>
        /// Gets or sets the response to the tab/fetch message.
        /// Possible values for the tab type include: 'continue', 'auth' or 'silentAuth'.
        /// </summary>
        /// <value>
        /// Cards in response to a <see cref="TabRequest"/>.
        /// </value>
        [JsonProperty(PropertyName = "tab")]
        public TabResponsePayload Tab { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
