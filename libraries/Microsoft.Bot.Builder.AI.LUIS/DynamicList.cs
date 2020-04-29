// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.Bot.Builder.AI.LuisV3;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.Luis
{
    /// <summary>
    /// Defines an extension for a list entity.
    /// </summary>
    public class DynamicList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicList"/> class.
        /// </summary>
        public DynamicList()
        {
        }

        /// <summary>
        /// Gets or sets the name of the list entity to extend.
        /// </summary>
        /// <value>
        /// The name of the list entity to extend.
        /// </value>
        [JsonProperty(PropertyName = "entity")]
        public string Entity { get; set; }

        /// <summary>
        /// Gets or sets the lists to append on the extended list entity.
        /// </summary>
        /// <value>
        /// The lists to append on the extended list entity.
        /// </value>
        [JsonProperty(PropertyName = "list")]
        public IList<ListElement> List { get; set; }
    }
}
