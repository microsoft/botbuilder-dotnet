// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;
    
    /// <summary>
    /// Current tab request context, i.e., the current theme.
    /// </summary>
    public class TabContext
    {
        /// <summary>
        /// Gets or sets the current user's theme.
        /// </summary>
        /// <value>
        /// The current user's theme.
        /// </value>
        [JsonProperty(PropertyName = "theme")]
        public string Theme { get; set; }
    }
}
