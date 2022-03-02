// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema.Teams
{
    /// <summary>
    /// Envelope for cards for a <see cref="TabResponse"/>.
    /// </summary>
    public class TabResponseCards
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TabResponseCards"/> class.
        /// </summary>
        public TabResponseCards()
        {
            CustomInit();
        }

        /// <summary>
        /// Gets or sets adaptive cards for this card tab response.
        /// </summary>
        /// <value>
        /// Cards for this <see cref="TabResponse"/>.
        /// </value>
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Property setter is required for the collection to be deserialized")]
        [JsonPropertyName("cards")]
        public IList<TabResponseCard> Cards { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        private void CustomInit()
        {
        }
    }
}
