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
    public class PropertyPaneChoiceGroupIconProperties
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyPaneChoiceGroupIconProperties"/> class.
        /// </summary>
        public PropertyPaneChoiceGroupIconProperties()
        {
            // Do nothing
        }

        /// <summary>
        /// Gets or Sets the name of the icon to use from the Office Fabric icon set of type <see cref="string"/>.
        /// </summary>
        [JsonProperty(PropertyName = "officeFabricIconFontName")]
        public string OfficeFabricIconFontName { get; set; }
    }
}
