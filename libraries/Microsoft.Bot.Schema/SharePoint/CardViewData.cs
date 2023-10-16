// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.SharePoint
{
    /// <summary>
    /// SharePoint Card View Data object.
    /// </summary>
    public class CardViewData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CardViewData"/> class.
        /// </summary>
        public CardViewData()
        {
            // Do nothing
        }

        /// <summary>
        /// Gets or sets the card view configuration.
        /// </summary>
        /// <value>Card view configuration.</value>
        [JsonProperty(PropertyName = "cardViewParameters")]
        public CardViewParameters CardViewParameters { get; set; }

        /// <summary>
        /// Gets or sets action to invoke when the card is selected.
        /// </summary>
        /// <value>Action to invoke.</value>
        [JsonProperty(PropertyName = "onCardSelection")]
        public Action OnCardSelection { get; set; }
    }
}
