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
        /// <value>This value is the aria label of the choice group.</value>
        [JsonProperty(PropertyName = "ariaLabel")]
        public string AriaLabel { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating whether the property pane choice group option is checked or not of type <see cref="bool"/>.
        /// </summary>
        /// <value>This value indicates whether the control is checked.</value>
        [JsonProperty(PropertyName = "checked")]
        public bool Checked { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating whether this control is enabled or not of type <see cref="bool"/>.
        /// </summary>
        /// <value>This value indicates whether the control is disabled.</value>
        [JsonProperty(PropertyName = "disabled")]
        public bool Disabled { get; set; }

        /// <summary>
        /// Gets or Sets the Icon component props for choice field of type <see cref="PropertyPaneChoiceGroupIconProperties"/>.
        /// </summary>
        /// <value>This value is the icon properties of the choice group.</value>
        [JsonProperty(PropertyName = "iconProps")]
        public PropertyPaneChoiceGroupIconProperties IconProps { get; set; }

        /// <summary>
        /// Gets or Sets the width and height of the image in px for choice field of type <see cref="PropertyPaneChoiceGroupImageSize"/>.
        /// </summary>
        /// <value>This value is the image size of the choice group.</value>
        [JsonProperty(PropertyName = "imageSize")]
        public PropertyPaneChoiceGroupImageSize ImageSize { get; set; }

        /// <summary>
        /// Gets or Sets the src of image for choice field of type <see cref="string"/>.
        /// </summary>
        /// <value>This value is the image source of the choice group.</value>
        [JsonProperty(PropertyName = "imageSrc")]
        public string ImageSrc { get; set; }

        /// <summary>
        /// Gets or Sets a key to uniquely identify this option of type <see cref="string"/>.
        /// </summary>
        /// <value>This value is the key of the choice group.</value>
        [JsonProperty(PropertyName = "key")]
        public string Key { get; set; }

        /// <summary>
        /// Gets or Sets text to render for this option of type <see cref="string"/>.
        /// </summary>
        /// <value>This value is the text of the choice group.</value>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }
    }
}
