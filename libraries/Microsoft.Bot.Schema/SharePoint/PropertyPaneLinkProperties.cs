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
    /// SharePoint property pane link properties object.
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
        /// Gets or Sets optional ariaLabel flag. Text for screen-reader to announce regardless of toggle state. Of type <see cref="string"/>.
        /// </summary>
        /// <value>This value is the aria label of the property pane link.</value>
        [JsonProperty(PropertyName = "ariaLabel")]
        public string AriaLabel { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating whether this control is enabled or not of type <see cref="bool"/>.
        /// </summary>
        /// <value>This value indicates whether the property pane link is disabled.</value>
        [JsonProperty(PropertyName = "disabled")]
        public bool Disabled { get; set; }

        /// <summary>
        /// Gets or Sets the location to which the link is targeted to of type <see cref="string"/>.
        /// </summary>
        /// <value>This value is the href of the property pane link.</value>
        [JsonProperty(PropertyName = "href")]
        public string Href { get; set; }

        /// <summary>
        /// Gets or Sets the props of popup window. of type <see cref="PropertyPaneLinkPopupWindowProperties"/>.
        /// </summary>
        /// <value>This value is the popup window properties of the property pane link.</value>
        [JsonProperty(PropertyName = "popupWindowProps")]
        public PropertyPaneLinkPopupWindowProperties PopupWindowProps { get; set; }

        /// <summary>
        /// Gets or Sets where to display the linked resource of type <see cref="string"/>.
        /// </summary>
        /// <value>This value is the target of the property pane link.</value>
        [JsonProperty(PropertyName = "target")]
        public string Target { get; set; }

        /// <summary>
        /// Gets or Sets the display text for the link of type <see cref="string"/>.
        /// </summary>
        /// <value>This value is the text of the property pane link.</value>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }
    }
}
