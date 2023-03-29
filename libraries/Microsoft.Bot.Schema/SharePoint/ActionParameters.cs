// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.SharePoint
{
    /// <summary>
    /// SharePoint action button parameters.
    /// </summary>
    public class ActionParameters
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionParameters"/> class.
        /// </summary>
        public ActionParameters()
        {
            // Do nothing
        }

        /// <summary>
        /// Gets or Sets the view of type <see cref="string"/>.
        /// </summary>
        [JsonProperty(PropertyName = "view")]
        public string View { get; set; }
    }
}
