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
    public class PropertyPaneLabelProperties : IPropertyPaneFieldProperties
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyPaneLabelProperties"/> class.
        /// </summary>
        public PropertyPaneLabelProperties()
        {
            // Do nothing
        }

        /// <summary>
        /// Gets or Sets the display text for the label of type <see cref="string"/>.
        /// </summary>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating whether the associated form field is required or not. of type <see cref="bool"/>.
        /// </summary>
        [JsonProperty(PropertyName = "required")]
        public bool Required { get; set; }
    }
}
