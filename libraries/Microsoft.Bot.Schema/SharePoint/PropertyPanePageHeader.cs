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
    public class PropertyPanePageHeader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyPanePageHeader"/> class.
        /// </summary>
        public PropertyPanePageHeader()
        {
            // Do nothing
        }

        /// <summary>
        /// Gets or Sets the description of type <see cref="string"/>.
        /// </summary>
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
    }
}
