// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using AdaptiveCards;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Schema.SharePoint
{
    /// <summary>
    /// SharePoint GetQuickView response object.
    /// </summary>
    public class GetQuickViewResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetQuickViewResponse"/> class.
        /// </summary>
        public GetQuickViewResponse()
        {
            // Do nothing
        }

        /// <summary>
        /// Gets or Sets data for the quick view of type <see cref="QuickViewData"/>.
        /// </summary>
        /// <value>This value is the data of the quick view response.</value>
        [JsonProperty(PropertyName = "data")]
        public QuickViewData Data { get; set; }

        /// <summary>
        /// Gets or Sets data for the quick view template of type <see cref="AdaptiveCard"/>.
        /// </summary>
        /// <value>This value is the template of the quick view response.</value>
        [JsonProperty(PropertyName = "template")]
        public AdaptiveCard Template { get; set; }

        /// <summary>
        /// Gets or Sets view Id of type <see cref="string"/>.
        /// </summary>
        /// <value>This value is the view Id of the quick view response.</value>
        [JsonProperty(PropertyName = "viewId")]
        public string ViewId { get; set; }

        /// <summary>
        /// Gets or Sets stackSize of type <see cref="int"/>.
        /// </summary>
        /// <value>This value is the stack size of the quick view response.</value>
        [JsonProperty(PropertyName = "stackSize")]
        public int StackSize { get; set; }
    }
}
