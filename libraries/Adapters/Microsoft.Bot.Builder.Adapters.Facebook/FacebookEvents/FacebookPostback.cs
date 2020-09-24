// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents
{
    /// <summary>
    /// Represents a Facebook Post Back.
    /// </summary>
    public class FacebookPostBack
    {
        /// <summary>
        /// Gets or sets the title of the post back message.
        /// </summary>
        /// <value>The title of the post back message.</value>
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the string to send back to the webhook.
        /// </summary>
        /// <value>The string to post back to the webhook.</value>
        [JsonProperty(PropertyName = "payload")]
        public string Payload { get; set; }

        /// <summary>
        /// Gets or sets the referral of the post back message.
        /// </summary>
        /// <value>The referral of the post back message.</value>
        [JsonProperty(PropertyName = "referral")]
        public string Referral { get; set; }
    }
}
