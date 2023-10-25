// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Bot.Schema.SharePoint
{
    /// <summary>
    /// Base class for Adaptive Card Extensions card view components.
    /// </summary>
    public class BaseCardComponent
    {
        /// <summary>
        /// Component name.
        /// </summary>
        [JsonProperty(PropertyName = "componentName")]
        private readonly CardComponentName cardComponentName;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCardComponent"/> class.
        /// </summary>
        /// <param name="cardComponentName">Component name.</param>
        protected BaseCardComponent(CardComponentName cardComponentName)
        {
            this.cardComponentName = cardComponentName;
        }

        /// <summary>
        /// Gets or sets optional unique identifier of the component's instance.
        /// </summary>
        /// <value>The value is a unique string identifier of the component.</value>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
    }
}
