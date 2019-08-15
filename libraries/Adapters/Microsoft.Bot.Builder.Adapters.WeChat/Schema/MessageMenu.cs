// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema
{
    public class MessageMenu
    {
        /// <summary>
        /// Gets or sets HeaderContent.
        /// </summary>
        /// <value>
        /// HeaderContent of the menu.
        /// </value>
        [JsonProperty("head_content")]
        public string HeaderContent { get; set; }

        /// <summary>
        /// Gets or sets MenuItems.
        /// </summary>
        /// <value>
        /// Items in message menu.
        /// </value>
        [JsonProperty("list")]
        public List<MenuItem> MenuItems { get; set; }

        /// <summary>
        /// Gets or sets TailContent.
        /// </summary>
        /// <value>
        /// Footer of the menu.
        /// </value>
        [JsonProperty("tail_content")]
        public string TailContent { get; set; }
    }
}
