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
    /// SharePoint property pane checkbox properties object.
    /// </summary>
    public class PropertyPaneCheckboxProperties : IPropertyPaneFieldProperties
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyPaneCheckboxProperties"/> class.
        /// </summary>
        public PropertyPaneCheckboxProperties()
        {
            // Do nothing
        }

        /// <summary>
        /// Gets or Sets the label to display next to the checkbox of type <see cref="string"/>.
        /// </summary>
        /// <value>This value is the text of the checkbox property.</value>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating whether this control is enabled or not of type <see cref="bool"/>.
        /// </summary>
        /// <value>This value indicates if the control is disabled.</value>
        [JsonProperty(PropertyName = "disabled")]
        public bool Disabled { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating whether the property pane checkbox is checked or not of type <see cref="bool"/>.
        /// </summary>
        /// <value>This value indicates if the control is checked.</value>
        [JsonProperty(PropertyName = "checked")]
        public bool Checked { get; set; }
    }
}
