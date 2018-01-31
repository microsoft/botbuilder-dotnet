// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// A message in a conversation
    /// </summary>
    public interface IMessageActivity : IActivity
    {
        /// <summary>
        /// The language code of the Text field
        /// </summary>
        /// <remarks>
        /// See https://msdn.microsoft.com/en-us/library/hh456380.aspx for a list of valid language codes
        /// </remarks>
        string Locale { get; set; }

        /// <summary>
        /// Content for the message
        /// </summary>
        string Text { get; set; }

        /// <summary>
        /// Speak tag (SSML markup for text to speech)
        /// </summary>
        string Speak { get; set; }

        /// <summary>
        /// Indicates whether the bot is accepting, expecting, or ignoring input
        /// </summary>
        string InputHint { get; set; }

        /// <summary>
        /// Text to display if the channel cannot render cards
        /// </summary>
        string Summary { get; set; }

        /// <summary>
        /// Format of text fields [plain|markdown] Default:markdown
        /// </summary>
        string TextFormat { get; set; }

        /// <summary>
        /// Hint for how to deal with multiple attachments: [list|carousel] Default:list
        /// </summary>
        string AttachmentLayout { get; set; }

        /// <summary>
        /// Attachments
        /// </summary>
        IList<Attachment> Attachments { get; set; }

        /// <summary>
        /// SuggestedActions are used to express actions for interacting with a card like keyboards/quickReplies
        /// </summary>
        SuggestedActions SuggestedActions { get; set; }

        /// <summary>
        /// Importance of the activity 
        /// Valid values are "low", "normal", and "high". Default value is "normal."
        /// </summary>
        string Importance { get; set; }

        /// <summary>
        /// Hint to describe how this activity should be delivered.  
        /// null or "default" = default delivery
        /// "notification" = notification semantics
        /// See DeliveryModes for current constants
        /// </summary>
        string DeliveryMode { get; set; }

        /// <summary>
        /// DateTime to expire the activity as ISO 8601 encoded datetime
        /// </summary>
        DateTimeOffset? Expiration { get; set; }

        /// <summary>
        /// Get mentions
        /// </summary>
        Mention[] GetMentions();

        /// <summary>
        /// Value provided with CardAction
        /// </summary>
        object Value { get; set; }

        /// <summary>
        /// True if this activity has text, attachments, or channelData
        /// </summary>
        bool HasContent();
    }
}
