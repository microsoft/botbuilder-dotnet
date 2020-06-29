// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Facebook
{
    /// <summary>
    /// Facebook message object used when sending messages via the Facebook API.
    /// </summary>
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
        /// <value>`true` if the message was sent from the page itself; otherwise, `false`.</value>
        [JsonProperty(PropertyName = "is_echo")]
        public bool IsEcho { get; set; }

        /// <summary>
        /// Gets or sets the message ID.
        /// </summary>
        /// <value>The message ID.</value>
        [JsonProperty(PropertyName = "mid")]
        public string Mid { get; set; }

        /// <summary>
        /// Newtonsoft JSON method for conditionally serializing the <see cref="QuickReplies"/> property.
        /// </summary>
        /// <returns>`true` to serialize the property; otherwise, `false`.</returns>
        public bool ShouldSerializeQuickReplies()
        {
            return QuickReplies.Count > 0;
        }

        /// <summary>
        /// Newtonsoft JSON method for conditionally serializing the <see cref="IsEcho"/> property.
        /// </summary>
        /// <returns>`true` to serialize the property; otherwise, `false`.</returns>
        public bool ShouldSerializeIsEcho()
        {
            return IsEcho;
        }

        /// <summary>
        /// Newtonsoft JSON method for conditionally serializing the <see cref="Attachments"/> property.
        /// </summary>
        /// <returns>`true` to serialize the property; otherwise, `false`.</returns>
        public bool ShouldSerializeAttachments()
        {
            return Attachments.Count > 0;
        }
    }
}
