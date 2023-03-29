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
    public class PropertyPaneToggleProperties : IPropertyPaneFieldProperties
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyPaneToggleProperties"/> class.
        /// </summary>
        public PropertyPaneToggleProperties()
        {
            // Do nothing
        }

        /// <summary>
        /// Gets or Sets the aria label of type <see cref="string"/>.
        /// </summary>
        [JsonProperty(PropertyName = "ariaLabel")]
        public string AriaLabel { get; set; }

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
        /// Gets or Sets a value indicating whether the property pane checkbox is checked or not of type <see cref="bool"/>.
        /// </summary>
        [JsonProperty(PropertyName = "checked")]
        public bool Checked { get; set; }

        /// <summary>
        /// Gets or Sets a key to uniquely identify the field of type <see cref="string"/>.
        /// </summary>
        [JsonProperty(PropertyName = "key")]
        public string Key { get; set; }

        /// <summary>
        /// Gets or Sets text to display when toggle is OFF of type <see cref="string"/>.
        /// </summary>
        [JsonProperty(PropertyName = "offText")]
        public string OffText { get; set; }

        /// <summary>
        /// Gets or Sets text to display when toggle is ON of type <see cref="string"/>.
        /// </summary>
        [JsonProperty(PropertyName = "onText")]
        public string OnText { get; set; }

        /// <summary>
        /// Gets or Sets text for screen-reader to announce when toggle is OFF of type <see cref="string"/>.
        /// </summary>
        [JsonProperty(PropertyName = "offAriaLabel")]
        public string OffAriaLabel { get; set; }

        /// <summary>
        /// Gets or Sets text for screen-reader to announce when toggle is ON of type <see cref="string"/>.
        /// </summary>
        [JsonProperty(PropertyName = "onAriaLabel")]
        public string OnAriaLabel { get; set; }
    }
}
