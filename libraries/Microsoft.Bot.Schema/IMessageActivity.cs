// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// A message in a conversation.
    /// </summary>
    public interface IMessageActivity : IActivity
    {
        /// <summary>
        /// Gets or sets the language code of the Text field.
        /// </summary>
        /// <remarks>
        /// See https://msdn.microsoft.com/library/hh456380.aspx for a list of valid language codes.
        /// </remarks>
        /// <value>
        /// The language code of the Text field.
        /// </value>
        string Locale { get; set; }

        /// <summary>
        /// Gets or sets content for the message.
        /// </summary>
        /// <value>
        /// Content for the message.
        /// </value>
        string Text { get; set; }

        /// <summary>
        /// Gets or sets speak tag (SSML markup for text to speech).
        /// </summary>
        /// <value>
        /// Speak tag (SSML markup for text to speech).
        /// </value>
        string Speak { get; set; }

        /// <summary>
        /// Gets or sets indicates whether the bot is accepting, expecting, or ignoring input.
        /// </summary>
        /// <value>
        /// Indicates whether the bot is accepting, expecting, or ignoring input.
        /// </value>
        string InputHint { get; set; }

        /// <summary>
        /// Gets or sets text to display if the channel cannot render cards.
        /// </summary>
        /// <value>
        /// Text to display if the channel cannot render cards.
        /// </value>
        string Summary { get; set; }

        /// <summary>
        /// Gets or sets format of text fields [plain|markdown] Default:markdown.
        /// </summary>
        /// <value>
        /// Format of text fields [plain|markdown] Default:markdown.
        /// </value>
        string TextFormat { get; set; }

        /// <summary>
        /// Gets or sets hint for how to deal with multiple attachments: [list|carousel] Default:list.
        /// </summary>
        /// <value>
        /// Hint for how to deal with multiple attachments: [list|carousel] Default:list.
        /// </value>
        string AttachmentLayout { get; set; }

        /// <summary>
        /// Gets or sets attachments.
        /// </summary>
        /// <value>
        /// Attachments.
        /// </value>
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        IList<Attachment> Attachments { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets or sets suggestedActions are used to express actions for interacting with a card like keyboards/quickReplies.
        /// </summary>
        /// <value>
        /// SuggestedActions are used to express actions for interacting with a card like keyboards/quickReplies.
        /// </value>
        SuggestedActions SuggestedActions { get; set; }

        /// <summary>
        /// Gets or sets importance of the activity
        /// Valid values are "low", "normal", and "high". Default value is "normal.".
        /// </summary>
        /// <value>
        /// Importance of the activity
        /// Valid values are "low", "normal", and "high". Default value is "normal.".
        /// </value>
        string Importance { get; set; }

        /// <summary>
        /// Gets or sets hint to describe how this activity should be delivered.
        /// null or "default" = default delivery
        /// "notification" = notification semantics
        /// See DeliveryModes for current constants.
        /// </summary>
        /// <value>
        /// Hint to describe how this activity should be delivered.
        /// null or "default" = default delivery
        /// "notification" = notification semantics
        /// See DeliveryModes for current constants.
        /// </value>
        string DeliveryMode { get; set; }

        /// <summary>
        /// Gets or sets dateTime to expire the activity as ISO 8601 encoded datetime.
        /// </summary>
        /// <value>
        /// DateTime to expire the activity as ISO 8601 encoded datetime.
        /// </value>
        DateTimeOffset? Expiration { get; set; }

        /// <summary>
        /// Gets or sets value provided with CardAction.
        /// </summary>
        /// <value>
        /// Value provided with CardAction.
        /// </value>
        object Value { get; set; }

        /// <summary>
        /// Get mentions.
        /// </summary>
        /// <returns>mentions.</returns>
        Mention[] GetMentions();

        /// <summary>
        /// True if this activity has text, attachments, or channelData.
        /// </summary>
        /// <returns>True or false.</returns>
        bool HasContent();
    }
}
