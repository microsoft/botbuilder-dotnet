// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents
{
    public class FacebookReferral
    {
        /// <summary>
        /// Gets or sets the title of the post back message.
        /// </summary>
        /// <value>The title of the post back message.</value>
        [JsonProperty(PropertyName = "ref")]
        public string Ref { get; set; }

        /// <summary>
        /// Gets or sets the string to send back to the webhook.
        /// </summary>
        /// <value>The string to post back to the webhook.</value>
        [JsonProperty(PropertyName = "source")]
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the referral of the post back message.
        /// </summary>
        /// <value>The referral of the post back message.</value>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
    }
}
