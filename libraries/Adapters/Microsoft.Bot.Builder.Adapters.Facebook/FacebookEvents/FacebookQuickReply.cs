// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents
{
    public class FacebookQuickReply
    {
        /// <summary>
        /// Gets or sets the content type of the reply. Can be:
        /// 'text' Sends a text button
        /// 'user_phone_number' Sends a button allowing recipient to send the phone number associated with their account.
        /// 'user_email' Sends a button allowing recipient to send the email associated with their account.
        /// </summary>
        /// <value>The content type.</value>
        [JsonProperty(PropertyName = "content_type")]
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the title of the reply. Required if content_type is 'text'.
        /// </summary>
        /// <value>The title of the reply. 20 character limit. </value>
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the payload of the reply. May be set to an empty string if image_url is set.
        /// </summary>
        /// <value>The payload. Can be either a string or a long.</value>
        [JsonProperty(PropertyName = "payload")]
        public object Payload { get; set; }

        /// <summary>
        /// Gets or sets the optional URL of the image to display on the quick reply button for text quick replies.
        /// Required if Title is an empty string.
        /// </summary>
        /// <value>The optional URL of the image to display.</value>
        [JsonProperty(PropertyName = "image_url")]
        public Uri ImageUrl { get; set; }
    }
}
