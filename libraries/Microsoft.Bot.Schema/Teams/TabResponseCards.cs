// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    
    /// <summary>
    /// Envelope for cards for a <see cref="TabResponse"/>.
    /// </summary>
    public class TabResponseCards
    {
        /// <summary>
        /// Gets adaptive cards for this card tab response.
        /// </summary>
        /// <value>
        /// Cards for this <see cref="TabResponse"/>.
        /// </value>
        [JsonProperty(PropertyName = "cards")]
        public IList<TabResponseCard> Cards { get; private set; } = new List<TabResponseCard>();
    }
}
