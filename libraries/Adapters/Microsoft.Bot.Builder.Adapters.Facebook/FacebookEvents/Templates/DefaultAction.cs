// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents.Templates
{
    /// <summary>
    /// Default action template.
    /// </summary>
    public class DefaultAction
    {
        /// <summary>
        /// Gets or sets the type of action.
        /// </summary>
        /// <value>The type of action.</value>
        [JsonProperty(PropertyName = "type")]
        public string ActionType { get; set; }

        /// <summary>
        /// Gets or sets the default URL to open.
        /// </summary>
        /// <value>The URL of the action.</value>
        [JsonProperty(PropertyName = "url")]
        public Uri ActionUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the action has extensions or not.
        /// </summary>
        /// <value>Indicates whether the action has extensions or not.</value>
        [JsonProperty(PropertyName = "messenger_extensions")]
        public bool MessengerExtensions { get; set; }

        /// <summary>
        /// Gets or sets the height ratio for the WebView. It can be "COMPACT", "TALL", or "FULL".
        /// </summary>
        /// <value>The height ratio for the WebView.</value>
        [JsonProperty(PropertyName = "webview_height_ratio")]
        public string WebviewHeightRatio { get; set; }
    }
}
