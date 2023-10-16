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
    public class CardViewResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CardViewResponse"/> class.
        /// </summary>
        public CardViewResponse()
        {
            // Do nothing
        }

        /// <summary>
        /// Gets or Sets AceData for the card view of type <see cref="AceData"/>.
        /// </summary>
        /// <value>This value is the ace data of the card view response.</value>
        [JsonProperty(PropertyName = "aceData")]
        public AceData AceData { get; set; }

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
        public IOnCardSelectionAction OnCardSelection { get; set; }

        /// <summary>
        /// Gets or Sets the view Id of type <see cref="string"/>.
        /// </summary>
        /// <value>This value is the view id of the card view.</value>
        [JsonProperty(PropertyName = "viewId")]
        public string ViewId { get; set; }
    }
}
