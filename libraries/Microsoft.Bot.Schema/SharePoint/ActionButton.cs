// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.SharePoint
{
    /// <summary>
    /// SharePoint action button.
    /// </summary>
    public class ActionButton
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionButton"/> class.
        /// </summary>
        public ActionButton()
        {
            // Do nothing
        }

        /// <summary>
        /// Gets or Sets the title of type <see cref="string"/>.
        /// </summary>
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or Sets the action of type <see cref="Action"/>.
        /// </summary>
        [JsonProperty(PropertyName = "action")]
        public Action Action { get; set; }
    }
}
