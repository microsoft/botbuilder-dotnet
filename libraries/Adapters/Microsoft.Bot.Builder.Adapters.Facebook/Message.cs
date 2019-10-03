// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents;

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
        /// <value>Attachment.</value>
        public FacebookAttachment Attachment { get; set; }

        /// <summary>
        /// Gets or sets the metadata.
        /// </summary>
        /// <value>Custom string that is delivered as a message echo. 1000 character limit.</value>
        public string Metadata { get; set; }

        /// <summary>
        /// Gets or sets the quick replies.
        /// </summary>
        /// <value>The quick replies array.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "it needs to be set in ActivityToFacebook method")]
        public List<object> QuickReplies { get; set; } = new List<object>();
    }
}
