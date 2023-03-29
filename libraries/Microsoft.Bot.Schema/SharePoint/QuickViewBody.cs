// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.SharePoint
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QuickViewBody"/> class.
    /// </summary>
    public class QuickViewBody
    {
        /// <summary>
        /// Gets or Sets the type of type <see cref="string"/>.
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating whether the separator exists.
        /// </summary>
        [JsonProperty(PropertyName = "separator")]
        public bool Separator { get; set; }

        /// <summary>
        /// Gets or Sets the items of type <see cref="QuickViewItem"/>.
        /// </summary>
        [JsonProperty(PropertyName = "items")]
        public IEnumerable<QuickViewItem> Items { get; set; }
    }
}
