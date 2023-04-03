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
    public class PropertyPaneDropDownOption
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyPaneDropDownOption"/> class.
        /// </summary>
        public PropertyPaneDropDownOption()
        {
            // Do nothing
        }

        /// <summary>
        /// This enum contains the different types of fields.
        /// </summary>
        public enum DropDownOptionType
        {
            /// <summary>
            /// Render normal menu item.
            /// </summary>
            Normal = 0,

            /// <summary>
            /// Render a divider.
            /// </summary>
            Divider = 1,

            /// <summary>
            /// Render menu item as a header.
            /// </summary>
            Header = 2
        }

        /// <summary>
        /// Gets or Sets index for this option of type <see cref="int"/>.
        /// </summary>
        /// <value>This value is the index of the drop down.</value>
        [JsonProperty(PropertyName = "index")]
        public int Index { get; set; }

        /// <summary>
        /// Gets or Sets a key to uniquely identify this option of type <see cref="string"/>.
        /// </summary>
        /// <value>This value is the key of the drop down.</value>
        [JsonProperty(PropertyName = "key")]
        public string Key { get; set; }

        /// <summary>
        /// Gets or Sets text to render for this option of type <see cref="string"/>.
        /// </summary>
        /// <value>This value is the text of the drop down.</value>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or Sets the type of option. If omitted, the default is PropertyPaneDropdownMenuItemType.Normal of type <see cref="DropDownOptionType"/>.
        /// </summary>
        /// <value>This value is the type of the drop down.</value>
        [JsonProperty(PropertyName = "type")]
        public DropDownOptionType Type { get; set; }
    }
}
