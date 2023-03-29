// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.SharePoint
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QuickViewTemplate"/> class.
    /// </summary>
    public class QuickViewTemplate
    {
        /// <summary>
        /// Gets or Sets the schema of type <see cref="string"/>.
        /// </summary>
        [JsonProperty(PropertyName = "$scehma")]
        public string Schema { get; set; }

        /// <summary>
        /// Gets or Sets the type of type <see cref="string"/>.
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or Sets the version of type <see cref="string"/>.
        /// </summary>
        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

        /// <summary>
        /// Gets or Sets the body of type <see cref="QuickViewBody"/>.
        /// </summary>
        [JsonProperty(PropertyName = "body")]
        public QuickViewBody Body { get; set; }
    }
}
