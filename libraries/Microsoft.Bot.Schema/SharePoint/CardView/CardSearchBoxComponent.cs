// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Bot.Schema.SharePoint
{
    /// <summary>
    /// Adaptive Card Extension search box component.
    /// </summary>
    public class CardSearchBoxComponent : BaseCardComponent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CardSearchBoxComponent"/> class.
        /// </summary>
        public CardSearchBoxComponent()
            : base(CardComponentName.SearchBox)
        {
        }

        /// <summary>
        /// Gets or sets the placeholder text to display in the sarch box.
        /// </summary>
        /// <value>Placeholder text to display.</value>
        [JsonProperty(PropertyName = "placeholder")]
        public string Placeholder { get; set; }

        /// <summary>
        /// Gets or sets the default text value of the search box.
        /// </summary>
        /// <value>Default value to display in the search box.</value>
        [JsonProperty(PropertyName = "defaultValue")]
        public string DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets the search box's button configuration.
        /// </summary>
        /// <value>Searh box's button configuration.</value>
        [JsonProperty(PropertyName = "button")]
        public CardSearchBoxButton Button { get; set; }
    }
}
