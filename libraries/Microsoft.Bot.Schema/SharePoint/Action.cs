// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.SharePoint
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ActionButton"/> class.
    /// </summary>
    public class Action
    {
        /// <summary>
        /// Gets or Sets the type of type <see cref="string"/>.
        /// </summary>
        /// <value>This value is the type of the action.</value>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or Sets the action parameters of type <see cref="string"/>.
        /// </summary>
        /// <value>This value is the parameters of the action.</value>
        [JsonProperty(PropertyName = "parameters")]
        public ActionParameters Parameters { get; set; }
    }
}
