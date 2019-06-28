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
        /// <value>
        /// The Type of the BotkitSlackMessage.
        /// </value>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the text of the message.
        /// </summary>
        /// <value>
        /// The Text of the BotkitSlackMessage.
        /// </value>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets any value field received from the platform.
        /// </summary>
        /// <value>
        /// The Value of the BotkitSlackMessage.
        /// </value>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of user who sent the message.
        /// </summary>
        /// <value>
        /// The User of the BotkitSlackMessage.
        /// </value>
        public string User { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the room/channel/space in which the message was sent.
        /// </summary>
        /// <value>
        /// The Channel of the BotkitSlackMessage.
        /// </value>
        public string Channel { get; set; }

        /// <summary>
        /// Gets or sets the AttachmentLayout.
        /// </summary>
        /// <value>
        /// The AttachmentLayout of the BotkitSlackMessage.
        /// </value>
        public string AttachmentLayout { get; set; }

        /// <summary>
        /// Gets or sets the speak indicator for the message.
        /// </summary>
        /// <value>
        /// The Speak of the BotkitSlackMessage.
        /// </value>
        public string Speak { get; set; }

        /// <summary>
        /// Gets or sets the input Hint for the message.
        /// </summary>
        /// <value>
        /// The InputHint of the BotkitSlackMessage.
        /// </value>
        public string InputHint { get; set; }

        /// <summary>
        /// Gets or sets the summary for the message.
        /// </summary>
        /// <value>
        /// The Summary of the BotkitSlackMessage.
        /// </value>
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the text format for the message.
        /// </summary>
        /// <value>
        /// The TextFormat of the BotkitSlackMessage.
        /// </value>
        public string TextFormat { get; set; }

        /// <summary>
        /// Gets or sets the importance of the message.
        /// </summary>
        /// <value>
        /// The Importance of the BotkitSlackMessage.
        /// </value>
        public string Importance { get; set; }

        /// <summary>
        /// Gets or sets the delivery mode for the message.
        /// </summary>
        /// <value>
        /// The DeliveryMode of the BotkitSlackMessage.
        /// </value>
        public string DeliveryMode { get; set; }

        /// <summary>
        /// Gets or sets the ChannelData object.
        /// </summary>
        /// <value>
        /// The ChannelData of the BotkitSlackMessage.
        /// </value>
        public object ChannelData { get; set; }

        /// <summary>
        /// Gets or sets the expiration date of the message.
        /// </summary>
        /// <value>
        /// The Expiration of the BotkitSlackMessage.
        /// </value>
        public DateTimeOffset Expiration { get; set; }

        /// <summary>
        /// Gets or sets the Reference object.
        /// </summary>
        /// <value>
        /// The Reference of the BotkitSlackMessage.
        /// </value>
        public ConversationReference Reference { get; set; }

        /// <summary>
        /// Gets or sets the activity representing the incoming message.
        /// </summary>
        /// <value>
        /// The IncomingMessage of the BotkitSlackMessage.
        /// </value>
        public Activity IncomingMessage { get; set; }

        /// <summary>
        /// Gets or sets a list of attachments.
        /// </summary>
        /// <value>
        /// The Attachments of the BotkitSlackMessage.
        /// </value>
        public IList<Attachment> Attachments { get; set; }

        /// <summary>
        /// Gets or sets the suggested actions.
        /// </summary>
        /// <value>
        /// The SuggestedActions of the BotkitSlackMessage.
        /// </value>
        public SuggestedActions SuggestedActions { get; set; }
    }
}
