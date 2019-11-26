// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Facebook
{
    public class Message
    {
        /// <summary>
        /// Gets or sets the text of the message.
        /// </summary>
        /// <value>The text of the message.</value>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the sticker ID.
        /// </summary>
        /// <value>The sticker ID.</value>
        [JsonProperty(PropertyName = "sticker_id")]
        public string StickerId { get; set; }

        /// <summary>
        /// Gets a list of attachments.
        /// </summary>
        /// <value>Attachments that could come with a Facebook message.</value>
        [JsonProperty(PropertyName = "attachments")]
        public List<FacebookAttachment> Attachments { get; } = new List<FacebookAttachment>();

        /// <summary>
        /// Gets or sets the attachment.
        /// </summary>
        /// <value>Single attachment that will be sent back to Facebook.</value>
        [JsonProperty(PropertyName = "attachment")]
        public FacebookAttachment Attachment { get; set; }

        /// <summary>
        /// Gets or sets the metadata.
        /// </summary>
        /// <value>Custom string that is delivered as a message echo. 1000 character limit.</value>
        [JsonProperty(PropertyName = "metadata")]
        public string Metadata { get; set; }

        /// <summary>
        /// Gets the quick replies.
        /// </summary>
        /// <value>The quick replies array.</value>
        [JsonProperty(PropertyName = "quick_replies")]
        public List<FacebookQuickReply> QuickReplies { get; } = new List<FacebookQuickReply>();

        /// <summary>
        /// Gets or sets a value indicating whether the message was sent from the page itself.
        /// </summary>
        /// <value>A value indicating whether the message was sent from the page itself.</value>
        [JsonProperty(PropertyName = "is_echo")]
        public bool IsEcho { get; set; }

        /// <summary>
        /// Gets or sets the Mid.
        /// </summary>
        /// <value>Message ID.</value>
        [JsonProperty(PropertyName = "mid")]
        public string Mid { get; set; }

        /// <summary>
        /// Newtonsoft Json method for conditionally serializing the QuickReplies property.
        /// </summary>
        /// <returns>A boolean with the value.</returns>
        public bool ShouldSerializeQuickReplies()
        {
            return QuickReplies.Count > 0;
        }

        /// <summary>
        /// Newtonsoft Json method for conditionally serializing the IsEcho property.
        /// </summary>
        /// <returns>A boolean with the value.</returns>
        public bool ShouldSerializeIsEcho()
        {
            return IsEcho;
        }

        public bool ShouldSerializeAttachments()
        {
            return Attachments.Count > 0;
        }
    }
}
