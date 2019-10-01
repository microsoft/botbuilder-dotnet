// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Adapters.Facebook
{
    public class Message
    {
        /// <summary>
        /// Gets or sets the text of the message.
        /// </summary>
        /// <value>The text of the message.</value>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the sticker ID.
        /// </summary>
        /// <value>The sticker ID.</value>
        public string StickerId { get; set; }

        /// <summary>
        /// Gets or sets the attachment.
        /// </summary>
        /// <value>The attachment.</value>
        public object Attachment { get; set; }

        /// <summary>
        /// Gets or sets the quick replies.
        /// </summary>
        /// <value>The quick replies array.</value>
        public List<object> QuickReplies { get; set; } = new List<object>();
    }
}
