// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.SharePoint
{
    /// <summary>
    /// SharePoint parameters for an quick view action.
    /// </summary>
    public class QuickViewActionParameters
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuickViewActionParameters"/> class.
        /// </summary>
        public QuickViewActionParameters()
        {
            // Do nothing
        }

        /// <summary>
        /// Gets or Sets the quick view id to be opened as part of the action of type <see cref="string"/>.
        /// </summary>
        /// <value>This value is quick view id to open.</value>
        [JsonProperty(PropertyName = "view")]
        public string View { get; set; }
    }
}
