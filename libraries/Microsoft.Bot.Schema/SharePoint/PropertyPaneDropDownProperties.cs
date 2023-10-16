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
    /// SharePoint property pane drop down properties object.
    /// </summary>
    public class PropertyPaneDropDownProperties : IPropertyPaneFieldProperties
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyPaneDropDownProperties"/> class.
        /// </summary>
        public PropertyPaneDropDownProperties()
        {
            // Do nothing
        }

        /// <summary>
        /// Gets or Sets the aria label of type <see cref="string"/>.
        /// </summary>
        /// <value>This value is the aria label of the drop down.</value>
        [JsonProperty(PropertyName = "ariaLabel")]
        public string AriaLabel { get; set; }

        /// <summary>
        /// Gets or Sets an element's number or position in the current set of controls. Maps to native aria-posinset attribute. It starts from 1 of type <see cref="int"/>.
        /// </summary>
        /// <value>This value is the aria position in set of the drop down.</value>
        [JsonProperty(PropertyName = "ariaPositionInSet")]
        public int AriaPositionInSet { get; set; }

        /// <summary>
        /// Gets or Sets the number of items in the current set of controls. Maps to native aria-setsize attribute of type <see cref="int"/>.
        /// </summary>
        /// <value>This value is the aria set size of the drop down.</value>
        [JsonProperty(PropertyName = "ariaSetSize")]
        public int AriaSetSize { get; set; }

        /// <summary>
        /// Gets or Sets the label of type <see cref="string"/>.
        /// </summary>
        /// <value>This value is the label of the drop down.</value>
        [JsonProperty(PropertyName = "label")]
        public string Label { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating whether this control is enabled or not of type <see cref="bool"/>.
        /// </summary>
        /// <value>This value indicates whether the property is disabled.</value>
        [JsonProperty(PropertyName = "disabled")]
        public bool Disabled { get; set; }

        /// <summary>
        /// Gets or Sets the error message of type <see cref="string"/>.
        /// </summary>
        /// <value>This value is the error message of the drop down.</value>
        [JsonProperty(PropertyName = "errorMessage")]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or Sets the key of the initially selected option of type <see cref="string"/>.
        /// </summary>
        /// <value>This value is the selected key of the drop down.</value>
        [JsonProperty(PropertyName = "selectedKey")]
        public string SelectedKey { get; set; }

        /// <summary>
        /// Gets or Sets the collection of options for this Dropdown of type <see cref="PropertyPaneDropDownOption"/>.
        /// </summary>
        /// <value>This value is the options of the drop down.</value>
        [JsonProperty(PropertyName = "options")]
        public IEnumerable<PropertyPaneDropDownOption> Options { get; set; }
    }
}
