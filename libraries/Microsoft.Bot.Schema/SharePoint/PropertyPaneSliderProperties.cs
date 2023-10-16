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
    /// SharePoint property pane slider properties object.
    /// </summary>
    public class PropertyPaneSliderProperties : IPropertyPaneFieldProperties
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyPaneSliderProperties"/> class.
        /// </summary>
        public PropertyPaneSliderProperties()
        {
            this.Step = 1;
        }

        /// <summary>
        /// Gets or Sets the label of type <see cref="string"/>.
        /// </summary>
        /// <value>This value is the label of the slider.</value>
        [JsonProperty(PropertyName = "label")]
        public string Label { get; set; }

        /// <summary>
        /// Gets or Sets the value of type <see cref="string"/>.
        /// </summary>
        /// <value>This value is the value of the slider.</value>
        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }

        /// <summary>
        /// Gets or Sets the aria label of type <see cref="string"/>.
        /// </summary>
        /// <value>This value is the aria label of the slider.</value>
        [JsonProperty(PropertyName = "ariaLabel")]
        public string AriaLabel { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating whether this control is enabled or not of type <see cref="bool"/>.
        /// </summary>
        /// <value>This value indicates whether the slider is disabled.</value>
        [JsonProperty(PropertyName = "disabled")]
        public bool Disabled { get; set; }

        /// <summary>
        /// Gets or Sets the max value of the Slider of type <see cref="int"/>.
        /// </summary>
        /// <value>This value is the max value of the slider.</value>
        [JsonProperty(PropertyName = "max")]
        public int Max { get; set; }

        /// <summary>
        /// Gets or Sets the min value of the Slider of type <see cref="int"/>.
        /// </summary>
        /// <value>This value is the min value of the slider.</value>
        [JsonProperty(PropertyName = "min")]
        public int Min { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating whether to show the value on the right of the Slider of type <see cref="bool"/>.
        /// </summary>
        /// <value>This value indicates whether the value of the slider should be shown.</value>
        [JsonProperty(PropertyName = "showValue")]
        public bool ShowValue { get; set; }

        /// <summary>
        /// Gets or Sets the  difference between the two adjacent values of the Slider. Defaults to 1. of type <see cref="int"/>.
        /// </summary>
        /// <value>This value is the step amount of the slider.</value>
        [JsonProperty(PropertyName = "step")]
        public int Step { get; set; }
    }
}
