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
    /// SharePoint property pane group object.
    /// </summary>
    public class PropertyPaneGroup : IPropertyPaneGroupOrConditionalGroup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyPaneGroup"/> class.
        /// </summary>
        public PropertyPaneGroup()
        {
            // Do nothing
        }

        /// <summary>
        /// Gets or Sets the group fields of type <see cref="PropertyPaneGroupField"/>.
        /// </summary>
        /// <value>This value is the group fields of the property pane group.</value>
        [JsonProperty(PropertyName = "groupFields")]
        public IEnumerable<PropertyPaneGroupField> GroupFields { get; set; }

        /// <summary>
        /// Gets or Sets the group name of type <see cref="string"/>.
        /// </summary>
        /// <value>This value is the group name of the property pane group.</value>
        [JsonProperty(PropertyName = "groupName")]
        public string GroupName { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating whether the PropertyPane group is collapsed or not of type <see cref="bool"/>.
        /// </summary>
        /// <value>This value indicates whether the property pane group is collapsed.</value>
        [JsonProperty(PropertyName = "isCollapsed")]
        public bool IsCollapsed { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating whether group name should be hidden of type <see cref="bool"/>.
        /// </summary>
        /// <value>This value indicates whether the property pane group is hidden.</value>
        [JsonProperty(PropertyName = "isGroupNameHidden")]
        public bool IsGroupNameHidden { get; set; }
    }
}
