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
    public class PropertyPanePage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyPanePage"/> class.
        /// </summary>
        public PropertyPanePage()
        {
            // Do nothing
        }

        /// <summary>
        /// Gets or Sets the groups of type <see cref="PropertyPaneGroup"/>.
        /// </summary>
        [JsonProperty(PropertyName = "groups")]
        public IEnumerable<IPropertyPaneGroupOrConditionalGroup> Groups { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating whether the groups on the PropertyPanePage are displayed as accordion or not of type <see cref="bool"/>.
        /// </summary>
        [JsonProperty(PropertyName = "displayGroupsAsAccordion")]
        public bool DisplayGroupsAsAccordion { get; set; }

        /// <summary>
        /// Gets or Sets the header for the property pane of type <see cref="PropertyPanePageHeader"/>.
        /// </summary>
        [JsonProperty(PropertyName = "header")]
        public PropertyPanePageHeader Header { get; set; }
    }
}
