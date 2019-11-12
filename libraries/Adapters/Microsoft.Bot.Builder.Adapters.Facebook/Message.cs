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
        /// Gets or sets a list of attachments.
        /// </summary>
        /// <value>Attachments.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "it needs to be set in ActivityToFacebook method")]
        [JsonProperty(PropertyName = "attachments")]
        public List<FacebookAttachment> Attachments { get; set; }

        /// <summary>
        /// Gets or sets the attachment.
        /// </summary>
        /// <value>Attachment.</value>
        [JsonProperty(PropertyName = "attachment")]
        public FacebookAttachment Attachment { get; set; }

        /// <summary>
        /// Gets or sets the metadata.
        /// </summary>
        /// <value>Custom string that is delivered as a message echo. 1000 character limit.</value>
        [JsonProperty(PropertyName = "metadata")]
        public string Metadata { get; set; }

        /// <summary>
        /// Gets or sets the quick replies.
        /// </summary>
        /// <value>The quick replies array.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "it needs to be set in ActivityToFacebook method")]
        [JsonProperty(PropertyName = "quick_replies")]
        public List<FacebookQuickReply> QuickReplies { get; set; } = new List<FacebookQuickReply>();

        [JsonProperty(PropertyName = "is_echo")]
        public bool IsEcho { get; set; }

        public bool ShouldSerializeQuickReplies()
        {
            return QuickReplies.Count > 0;
        }

        public bool ShouldSerializeIsEcho()
        {
            return IsEcho;
        }
    }
}
