// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.SharePoint
{
    /// <summary>
    /// Base class for Adaptive Card Extensions card view components.
    /// </summary>
    public class BaseCardComponent
    {
        /// <summary>
        /// Gets or sets component name.
        /// </summary>
        /// <value>The value is the unique name of the component type.</value>
        [JsonProperty(PropertyName = "componentName")]
        public CardComponentName ComponentName { get; protected set; }

        /// <summary>
        /// Gets or sets optional unique identifier of the component's instance.
        /// </summary>
        /// <value>The value is a unique string identifier of the component.</value>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
    }
}
