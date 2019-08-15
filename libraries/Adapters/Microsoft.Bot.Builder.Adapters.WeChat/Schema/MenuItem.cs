// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema
{
    public class MenuItem
    {
        /// <summary>
        /// Gets or sets Id.
        /// </summary>
        /// <value>
        /// Id of the menu item.
        /// </value>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets Content.
        /// </summary>
        /// <value>
        /// Content of the menu item.
        /// </value>
        [JsonProperty("content")]
        public string Content { get; set; }
    }
}
