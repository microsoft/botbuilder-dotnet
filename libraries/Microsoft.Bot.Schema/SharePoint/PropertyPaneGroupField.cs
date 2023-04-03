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
    public class PropertyPaneGroupField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyPaneGroupField"/> class.
        /// </summary>
        public PropertyPaneGroupField()
        {
            // Do nothing
        }

        /// <summary>
        /// This enum contains the different types of fields.
        /// </summary>
        public enum FieldType
        {
            /// <summary>
            /// Checkbox field.
            /// </summary>
            CheckBox = 2,

            /// <summary>
            /// TextField field.
            /// </summary>
            TextField = 3,

            /// <summary>
            /// Toggle field.
            /// </summary>
            Toggle = 5,

            /// <summary>
            /// Dropdown field.
            /// </summary>
            Dropdown = 6,

            /// <summary>
            /// Label field.
            /// </summary>
            Label = 7,

            /// <summary>
            /// Slider field.
            /// </summary>
            Slider = 8,

            /// <summary>
            /// ChoiceGroup field.
            /// </summary>
            ChoiceGroup = 10,

            /// <summary>
            /// Horizontal Rule field.
            /// </summary>
            HorizontalRule = 12,

            /// <summary>
            /// Link field.
            /// </summary>
            Link = 13
        }

        /// <summary>
        /// Gets or Sets the type of field <see cref="FieldType"/> enum.
        /// </summary>
        /// <value>This value is the type of the property pane field.</value>
        [JsonProperty(PropertyName = "type")]
        public FieldType Type { get; set; }

        /// <summary>
        /// Gets or Sets the properties property of type <see cref="IPropertyPaneFieldProperties"/>.
        /// </summary>
        /// <value>This value is the properties of the property pane field.</value>
        [JsonProperty(PropertyName = "properties")]
        public IPropertyPaneFieldProperties Properties { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating whether this control should be focused of type <see cref="bool"/>.
        /// </summary>
        /// <value>This value indicates whether the property pane field should focus.</value>
        [JsonProperty(PropertyName = "shouldFocus")]
        public bool ShouldFocus { get; set; }

        /// <summary>
        /// Gets or Sets the target property of type <see cref="string"/>.
        /// </summary>
        /// <value>This value is the target property of the property pane field.</value>
        [JsonProperty(PropertyName = "targetProperty")]
        public string TargetProperty { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating whether group name should be hidden of type <see cref="bool"/>.
        /// </summary>
        /// <value>This value indicates whether the property pane field group name is hidden.</value>
        [JsonProperty(PropertyName = "isGroupNameHidden")]
        public bool IsGroupNameHidden { get; set; }
    }
}
