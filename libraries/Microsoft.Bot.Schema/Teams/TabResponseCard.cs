// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;
    
    /// <summary>
    /// Envelope for cards for a Tab request.
    /// </summary>
    public class TabResponseCard
    {
        /// <summary>
        /// Gets or sets adaptive card for this card tab response.
        /// </summary>
        /// <value>
        /// Cards for this <see cref="TabResponse"/>.
        /// </value>
        [JsonProperty(PropertyName = "card")]
        public object Card { get; set; }
    }
}
