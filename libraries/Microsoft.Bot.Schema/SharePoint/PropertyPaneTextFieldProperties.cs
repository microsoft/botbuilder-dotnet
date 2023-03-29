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
    public class PropertyPaneTextFieldProperties : IPropertyPaneFieldProperties
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyPaneTextFieldProperties"/> class.
        /// </summary>
        public PropertyPaneTextFieldProperties()
        {
            // Do nothing
        }

        /// <summary>
        /// Gets or Sets the label of type <see cref="string"/>.
        /// </summary>
        [JsonProperty(PropertyName = "label")]
        public string Label { get; set; }

        /// <summary>
        /// Gets or Sets the value of type <see cref="string"/>.
        /// </summary>
        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }

        /// <summary>
        /// Gets or Sets the aria label of type <see cref="string"/>.
        /// </summary>
        [JsonProperty(PropertyName = "ariaLabel")]
        public string AriaLabel { get; set; }

        /// <summary>
        /// Gets or Sets the amount of time to wait before validating after the users stop typing in ms of type <see cref="int"/>.
        /// </summary>
        [JsonProperty(PropertyName = "deferredValidationTime")]
        public int DeferredValidationTime { get; set; }

        /// <summary>
        /// Gets or Sets the description of type <see cref="string"/>.
        /// </summary>
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

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
        /// Gets or Sets the name used to log PropertyPaneTextField value changes for engagement tracking of type <see cref="string"/>.
        /// </summary>
        [JsonProperty(PropertyName = "logName")]
        public string LogName { get; set; }

        /// <summary>
        /// Gets or Sets the maximum number of characters that the PropertyPaneTextField can have of type <see cref="int"/>.
        /// </summary>
        [JsonProperty(PropertyName = "maxLength")]
        public int MaxLength { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating whether or not the text field is a multiline text field of type <see cref="bool"/>.
        /// </summary>
        [JsonProperty(PropertyName = "multiline")]
        public bool Multiline { get; set; }

        /// <summary>
        /// Gets or Sets the placeholder text to be displayed in the text field of type <see cref="string"/>.
        /// </summary>
        [JsonProperty(PropertyName = "placeholder")]
        public string Placeholder { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating whether or not the multiline text field is resizable of type <see cref="bool"/>.
        /// </summary>
        [JsonProperty(PropertyName = "resizable")]
        public bool Resizable { get; set; }

        /// <summary>
        /// Gets or Sets the value that specifies the visible height of a text area(multiline text TextField), in lines.maximum number of characters that the PropertyPaneTextField can have of type <see cref="int"/>.
        /// </summary>
        [JsonProperty(PropertyName = "rows")]
        public int Rows { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating whether or not the text field is underlined of type <see cref="bool"/>.
        /// </summary>
        [JsonProperty(PropertyName = "underlined")]
        public bool Underlined { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating whether to run validation when the PropertyPaneTextField is focused of type <see cref="bool"/>.
        /// </summary>
        [JsonProperty(PropertyName = "validateOnFocusIn")]
        public bool ValidateOnFocusIn { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating whether to run validation when the PropertyPaneTextField is out of focus or on blur of type <see cref="bool"/>.
        /// </summary>
        [JsonProperty(PropertyName = "validateOnFocusOut")]
        public bool ValidateOnFocusOut { get; set; }
    }
}
