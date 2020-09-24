// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents
{
    /// <summary>
    /// Facebook Quick Reply object that can be sent as part of a Facebook message.
    /// </summary>
    public class FacebookQuickReply
    {
        /// <summary>
        /// Gets or sets the content type of the reply. Can be:
        /// - "text", which sends a text button.
        /// - "user_phone_number", which sends a button allowing the recipient to send the phone number associated with their account.
        /// - "user_email", which sends a button allowing the recipient to send the email associated with their account.
        /// </summary>
        /// <value>The content type.</value>
        [JsonProperty(PropertyName = "content_type")]
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the title of the reply. Required if content_type is "text".
        /// </summary>
        /// <value>The title of the reply. 20 character limit. </value>
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the payload of the reply. May be set to an empty string if the <see cref="ImageUrl"/> property is set.
        /// </summary>
        /// <value>The payload. Can be either a string or a long.</value>
        [JsonProperty(PropertyName = "payload")]
        public object Payload { get; set; }

        /// <summary>
        /// Gets or sets the optional URL of the image to display on the quick reply button for text quick replies.
        /// Required if the <see cref="Title"/> property is empty.
        /// </summary>
        /// <value>The optional URL of the image to display.</value>
        [JsonProperty(PropertyName = "image_url")]
        public Uri ImageUrl { get; set; }
    }
}
