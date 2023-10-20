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
    /// Adaptive Card Extension card bar component.
    /// </summary>
    public class CardBarComponent : BaseCardComponent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CardBarComponent"/> class.
        /// </summary>
        public CardBarComponent()
            : base(CardComponentName.CardBar)
        {
        }

        /// <summary>
        /// Gets or sets the title to display.
        /// </summary>
        /// <value>Title value to display in the card bar.</value>
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the icon to display.
        /// </summary>
        /// <value>Icon to display in the card bar.</value>
        [JsonProperty(PropertyName = "icon")]
        public CardImage Icon { get; set; }
    }
}
