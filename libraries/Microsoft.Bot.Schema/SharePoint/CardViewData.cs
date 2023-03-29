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
        /// Gets or Sets the action buttons of type <see cref="ActionButton"/>.
        /// </summary>
        [JsonProperty(PropertyName = "actionButtons")]
        public IEnumerable<ActionButton> ActionButtons { get; set; }

        /// <summary>
        /// Gets or Sets the primary text of type <see cref="string"/>.
        /// </summary>
        [JsonProperty(PropertyName = "primaryText")]
        public string PrimaryText { get; set; }
    }
}
