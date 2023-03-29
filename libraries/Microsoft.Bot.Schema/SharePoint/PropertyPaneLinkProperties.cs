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
    public class PropertyPaneLinkProperties : IPropertyPaneFieldProperties
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyPaneLinkProperties"/> class.
        /// </summary>
        public PropertyPaneLinkProperties()
        {
            // Do nothing
        }

        /// <summary>
        /// Gets or Sets the aria label of type <see cref="string"/>.
        /// </summary>
        [JsonProperty(PropertyName = "ariaLabel")]
        public string AriaLabel { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating whether this control is enabled or not of type <see cref="bool"/>.
        /// </summary>
        [JsonProperty(PropertyName = "disabled")]
        public bool Disabled { get; set; }

        /// <summary>
        /// Gets or Sets the location to which the link is targeted to of type <see cref="string"/>.
        /// </summary>
        [JsonProperty(PropertyName = "href")]
        public string Href { get; set; }

        /// <summary>
        /// Gets or Sets the props of pop up window. of type <see cref="PropertyPaneLinkPopupWindowProperties"/>.
        /// </summary>
        [JsonProperty(PropertyName = "popupWindowProps")]
        public PropertyPaneLinkPopupWindowProperties PopupWindowProps { get; set; }

        /// <summary>
        /// Gets or Sets where to display the linked resource of type <see cref="string"/>.
        /// </summary>
        [JsonProperty(PropertyName = "target")]
        public string Target { get; set; }

        /// <summary>
        /// Gets or Sets the display text for the link of type <see cref="string"/>.
        /// </summary>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }
    }
}
