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
        [JsonProperty(PropertyName = "ariaLabel")]
        public string AriaLabel { get; set; }

        /// <summary>
        /// Gets or Sets an element's number or position in the current set of controls. Maps to native aria-posinset attribute. It starts from 1 of type <see cref="int"/>.
        /// </summary>
        [JsonProperty(PropertyName = "ariaPositionInSet")]
        public int AriaPositionInSet { get; set; }

        /// <summary>
        /// Gets or Sets the number of items in the current set of controls. Maps to native aria-setsize attribute of type <see cref="int"/>.
        /// </summary>
        [JsonProperty(PropertyName = "ariaSetSize")]
        public int AriaSetSize { get; set; }

        /// <summary>
        /// Gets or Sets the label of type <see cref="string"/>.
        /// </summary>
        [JsonProperty(PropertyName = "label")]
        public string Label { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating whether this control is enabled or not of type <see cref="bool"/>.
        /// </summary>
        [JsonProperty(PropertyName = "disabled")]
        public bool Disabled { get; set; }

        /// <summary>
        /// Gets or Sets the error message of type <see cref="string"/>.
        /// </summary>
        [JsonProperty(PropertyName = "errorMessage")]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or Sets the key of the initially selected option of type <see cref="string"/>.
        /// </summary>
        [JsonProperty(PropertyName = "selectedKey")]
        public string SelectedKey { get; set; }

        /// <summary>
        /// Gets or Sets the collection of options for this Dropdown of type <see cref="PropertyPaneDropDownOption"/>.
        /// </summary>
        [JsonProperty(PropertyName = "options")]
        public IEnumerable<PropertyPaneDropDownOption> Options { get; set; }
    }
}
