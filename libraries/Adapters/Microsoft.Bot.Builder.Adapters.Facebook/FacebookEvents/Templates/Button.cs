// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents.Templates
{
    /// <summary>
    /// Facebook button object.
    /// </summary>
    public class Button
    {
        /// <summary>
        /// Gets or sets the type of button.
        /// </summary>
        /// <value>The type of the button.</value>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the URL of the button.
        /// </summary>
        /// <value>The URL of the button.</value>
        [JsonProperty(PropertyName = "url")]
        public Uri Url { get; set; }

        /// <summary>
        /// Gets or sets the title of the button.
        /// </summary>
        /// <value>The title of the button.</value>
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the string sent to webhook.
        /// </summary>
        /// <value>The string sent to webhook.</value>
        [JsonProperty(PropertyName = "payload")]
        public string Payload { get; set; }
    }
}
