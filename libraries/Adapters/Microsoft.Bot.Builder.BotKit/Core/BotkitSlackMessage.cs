// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.BotKit.Core
{
    /// <summary>
    /// Defines the expected form of a message or event object being handled by Botkit.
    /// </summary>
    public class BotkitSlackMessage : IBotkitMessage
    {
        /// <summary>
        /// Gets or sets the type of event, in most cases defined by the messaging channel or adapter.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the text of the message.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets any value field received from the platform.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of user who sent the message.
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the room/channel/space in which the message was sent.
        /// </summary>
        public string Channel { get; set; }

        /// <summary>
        /// Gets or sets the AttachmentLayout.
        /// </summary>
        public string AttachmentLayout { get; set; }

        /// <summary>
        /// Gets or sets the speak indicator for the message.
        /// </summary>
        public string Speak { get; set; }

        /// <summary>
        /// Gets or sets the input Hint for the message.
        /// </summary>
        public string InputHint { get; set; }

        /// <summary>
        /// Gets or sets the summary for the message.
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the text format for the message.
        /// </summary>
        public string TextFormat { get; set; }

        /// <summary>
        /// Gets or sets the importance of the message.
        /// </summary>
        public string Importance { get; set; }

        /// <summary>
        /// Gets or sets the delivery mode for the message.
        /// </summary>
        public string DeliveryMode { get; set; }

        /// <summary>
        /// Gets or sets the ChannelData object.
        /// </summary>
        public object ChannelData { get; set; }

        /// <summary>
        /// Gets or sets the expiration date of the message.
        /// </summary>
        public DateTimeOffset Expiration { get; set; }

        /// <summary>
        /// Gets or sets the Reference object.
        /// </summary>
        public ConversationReference Reference { get; set; }

        /// <summary>
        /// Gets or sets the activity representing the incoming message.
        /// </summary>
        public Activity IncomingMessage { get; set; }

        /// <summary>
        /// Gets or sets a list of attachments.
        /// </summary>
        public IList<Attachment> Attachments { get; set; }

        /// <summary>
        /// Gets or sets the suggested actions.
        /// </summary>
        public SuggestedActions SuggestedActions { get; set; }
    }
}
