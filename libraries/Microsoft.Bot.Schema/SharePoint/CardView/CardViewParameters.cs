// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.SharePoint
{
    /// <summary>
    /// Adaptive Card Extension Card View Parameters.
    /// </summary>
    public class CardViewParameters
    {
        /// <summary>
        /// Gets or sets card view type.
        /// </summary>
        /// <value>Card view type.</value>
        [JsonProperty(PropertyName = "cardViewType")]
        public string CardViewType { get; set; }

        /// <summary>
        /// Gets or sets image displayed on the card.
        /// </summary>
        /// <value>Image displayed on the card.</value>
        [JsonProperty(PropertyName = "image")]
        public CardImage Image { get; set; }

        /// <summary>
        /// Gets card view title area (card bar) components.
        /// </summary>
        /// <value>Card bar area components.</value>
        [JsonProperty(PropertyName = "cardBar")]
        public IEnumerable<CardBarComponent> CardBar { get; } = new CardBarComponent[1];

        /// <summary>
        /// Gets or sets card view header area components.
        /// </summary>
        /// <value>Card header area components.</value>
        [JsonProperty(PropertyName = "header")]
        public IEnumerable<BaseCardComponent> Header { get; set; }

        /// <summary>
        /// Gets or sets card view body area components.
        /// </summary>
        /// <value>Card body area components.</value>
        [JsonProperty(PropertyName = "body")]
        public IEnumerable<BaseCardComponent> Body { get; set; }

        /// <summary>
        /// Gets or sets card footer area components.
        /// </summary>
        /// <value>Card footer area components.</value>
        [JsonProperty(PropertyName = "footer")]
        public IEnumerable<BaseCardComponent> Footer { get; set; }
    }
}
