// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using static Microsoft.Bot.Schema.SharePoint.PropertyPaneDropDownOption;

namespace Microsoft.Bot.Schema.SharePoint
{
    /// <summary>
    /// SharePoint Quick View Data object.
    /// </summary>
    public class PropertyPaneChoiceGroupOption
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyPaneChoiceGroupOption"/> class.
        /// </summary>
        public PropertyPaneChoiceGroupOption()
        {
            // Do nothing
        }

        /// <summary>
        /// Gets or Sets the aria label of type <see cref="string"/>.
        /// </summary>
        [JsonProperty(PropertyName = "ariaLabel")]
        public string AriaLabel { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating whether the property pane choice group option is checked or not of type <see cref="bool"/>.
        /// </summary>
        [JsonProperty(PropertyName = "checked")]
        public bool Checked { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating whether this control is enabled or not of type <see cref="bool"/>.
        /// </summary>
        [JsonProperty(PropertyName = "disabled")]
        public bool Disabled { get; set; }

        /// <summary>
        /// Gets or Sets the Icon component props for choice field of type <see cref="PropertyPaneChoiceGroupIconProperties"/>.
        /// </summary>
        [JsonProperty(PropertyName = "iconProps")]
        public PropertyPaneChoiceGroupIconProperties IconProps { get; set; }

        /// <summary>
        /// Gets or Sets the width and height of the image in px for choice field of type <see cref="PropertyPaneChoiceGroupImageSize"/>.
        /// </summary>
        [JsonProperty(PropertyName = "imageSize")]
        public PropertyPaneChoiceGroupImageSize ImageSize { get; set; }

        /// <summary>
        /// Gets or Sets the src of image for choice field of type <see cref="string"/>.
        /// </summary>
        [JsonProperty(PropertyName = "imageSrc")]
        public string ImageSrc { get; set; }

        /// <summary>
        /// Gets or Sets a key to uniquely identify this option of type <see cref="string"/>.
        /// </summary>
        [JsonProperty(PropertyName = "key")]
        public string Key { get; set; }

        /// <summary>
        /// Gets or Sets text to render for this option of type <see cref="string"/>.
        /// </summary>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }
    }
}
