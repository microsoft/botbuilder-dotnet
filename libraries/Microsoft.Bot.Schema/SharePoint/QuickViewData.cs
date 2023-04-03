// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Schema.SharePoint
{
    /// <summary>
    /// SharePoint Quick View Data object.
    /// </summary>
    public class QuickViewData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuickViewData"/> class.
        /// </summary>
        public QuickViewData()
        {
            // Do nothing
        }

        /// <summary>
        /// Gets or Sets the title of type <see cref="string"/>.
        /// </summary>
        /// <value>This value is the title of the quick view data.</value>
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or Sets the description of type <see cref="string"/>.
        /// </summary>
        /// <value>This value is the description of the quick view data.</value>
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
    }
}
