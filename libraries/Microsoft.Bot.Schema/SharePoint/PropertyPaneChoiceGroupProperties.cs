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
    /// SharePoint property pane choice group properties object.
    /// </summary>
    public class PropertyPaneChoiceGroupProperties : IPropertyPaneFieldProperties
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyPaneChoiceGroupProperties"/> class.
        /// </summary>
        public PropertyPaneChoiceGroupProperties()
        {
            // Do nothing
        }

        /// <summary>
        /// Gets or Sets the label of type <see cref="string"/>.
        /// </summary>
        /// <value>This value is the label of the choice group.</value>
        [JsonProperty(PropertyName = "label")]
        public string Label { get; set; }

        /// <summary>
        /// Gets or Sets the collection of options for this choice group of type <see cref="PropertyPaneChoiceGroupOption"/>.
        /// </summary>
        /// <value>This value is the icon properties of the choice group.</value>
        [JsonProperty(PropertyName = "options")]
        public IEnumerable<PropertyPaneChoiceGroupOption> Options { get; set; }
    }
}
