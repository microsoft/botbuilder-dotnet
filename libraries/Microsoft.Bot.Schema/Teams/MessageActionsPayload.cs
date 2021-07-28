// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the individual message within a chat or channel where a
    /// message actions is taken.
    /// </summary>
    public partial class MessageActionsPayload
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageActionsPayload"/> class.
        /// </summary>
        public MessageActionsPayload()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageActionsPayload"/> class.
        /// </summary>
        /// <param name="id">Unique id of the message.</param>
        /// <param name="replyToId">Id of the parent/root message of the
        /// thread.</param>
        /// <param name="messageType">Type of message - automatically set to
        /// message. Possible values include: 'message'.</param>
        /// <param name="createdDateTime">Timestamp of when the message was
        /// created.</param>
        /// <param name="lastModifiedDateTime">Timestamp of when the message
        /// was edited or updated.</param>
        /// <param name="deleted">Indicates whether a message has been soft
        /// deleted.</param>
        /// <param name="subject">Subject line of the message.</param>
        /// <param name="summary">Summary text of the message that could be
        /// used for notifications.</param>
        /// <param name="importance">The importance of the message. Possible
        /// values include: 'normal', 'high', 'urgent'.</param>
        /// <param name="locale">Locale of the message set by the
        /// client.</param>
        /// <param name="from">Sender of the message.</param>
        /// <param name="body">Plaintext/HTML representation of the content of
        /// the message.</param>
        /// <param name="attachmentLayout">How the attachment(s) are displayed
        /// in the message.</param>
        /// <param name="attachments">Attachments in the message - card, image,
        /// file, etc.</param>
        /// <param name="mentions">List of entities mentioned in the
        /// message.</param>
        /// <param name="reactions">Reactions for the message.</param>
        public MessageActionsPayload(string id = default, string replyToId = default, string messageType = default, string createdDateTime = default, string lastModifiedDateTime = default, bool? deleted = default, string subject = default, string summary = default, string importance = default, string locale = default, MessageActionsPayloadFrom from = default, MessageActionsPayloadBody body = default, string attachmentLayout = default, IList<MessageActionsPayloadAttachment> attachments = default, IList<MessageActionsPayloadMention> mentions = default, IList<MessageActionsPayloadReaction> reactions = default)
        {
            Id = id;
            ReplyToId = replyToId;
            MessageType = messageType;
            CreatedDateTime = createdDateTime;
            LastModifiedDateTime = lastModifiedDateTime;
            Deleted = deleted;
            Subject = subject;
            Summary = summary;
            Importance = importance;
            Locale = locale;
            From = from;
            Body = body;
            AttachmentLayout = attachmentLayout;
            Attachments = attachments;
            Mentions = mentions;
            Reactions = reactions;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets unique id of the message.
        /// </summary>
        /// <value>The message ID.</value>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets id of the parent/root message of the thread.
        /// </summary>
        /// <value>The ID of the parent/root message of the thread.</value>
        [JsonProperty(PropertyName = "replyToId")]
        public string ReplyToId { get; set; }

        /// <summary>
        /// Gets or sets type of message - automatically set to message.
        /// Possible values include: 'message'.
        /// </summary>
        /// <value>The message type.</value>
        [JsonProperty(PropertyName = "messageType")]
        public string MessageType { get; set; }

        /// <summary>
        /// Gets or sets timestamp of when the message was created.
        /// </summary>
        /// <value>The timestamp of when the message was created.</value>
        [JsonProperty(PropertyName = "createdDateTime")]
        public string CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets timestamp of when the message was edited or updated.
        /// </summary>
        /// <value>The timestamp of when the message was edited or updated.</value>
        [JsonProperty(PropertyName = "lastModifiedDateTime")]
        public string LastModifiedDateTime { get; set; }

        /// <summary>
        /// Gets or sets indicates whether a message has been soft deleted.
        /// </summary>
        /// <value>Boolean indicating whether a message has been soft deleted.</value>
        [JsonProperty(PropertyName = "deleted")]
        public bool? Deleted { get; set; }

        /// <summary>
        /// Gets or sets subject line of the message.
        /// </summary>
        /// <value>The subject line of the message.</value>
        [JsonProperty(PropertyName = "subject")]
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets summary text of the message that could be used for
        /// notifications.
        /// </summary>
        /// <value>The summary text of the message.</value>
        [JsonProperty(PropertyName = "summary")]
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the importance of the message. Possible values
        /// include: 'normal', 'high', 'urgent'.
        /// </summary>
        /// <value>The importance of the message.</value>
        [JsonProperty(PropertyName = "importance")]
        public string Importance { get; set; }

        /// <summary>
        /// Gets or sets locale of the message set by the client.
        /// </summary>
        /// <value>The locale of the message set by the client.</value>
        [JsonProperty(PropertyName = "locale")]
        public string Locale { get; set; }

        /// <summary>
        /// Gets or sets sender of the message.
        /// </summary>
        /// <value>The sender of the message.</value>
        [JsonProperty(PropertyName = "from")]
        public MessageActionsPayloadFrom From { get; set; }

        /// <summary>
        /// Gets or sets plaintext/HTML representation of the content of the
        /// message.
        /// </summary>
        /// <value>The plaintext/HTML representation of the content of the message.</value>
        [JsonProperty(PropertyName = "body")]
        public MessageActionsPayloadBody Body { get; set; }

        /// <summary>
        /// Gets or sets how the attachment(s) are displayed in the message.
        /// </summary>
        /// <value>String idicating how the attachment(s) are displayed in the message.</value>
        [JsonProperty(PropertyName = "attachmentLayout")]
        public string AttachmentLayout { get; set; }

        /// <summary>
        /// Gets or sets attachments in the message - card, image, file, etc.
        /// </summary>
        /// <value>The attachments in the message.</value>
        [JsonProperty(PropertyName = "attachments")]
#pragma warning disable CA2227 // Collection properties should be read only  (we can't change this without breaking compat)
        public IList<MessageActionsPayloadAttachment> Attachments { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets or sets list of entities mentioned in the message.
        /// </summary>
        /// <value>The entities mentioned in the message.</value>
        [JsonProperty(PropertyName = "mentions")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking compat)
        public IList<MessageActionsPayloadMention> Mentions { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets or sets reactions for the message.
        /// </summary>
        /// <value>The reactions for the message.</value>
        [JsonProperty(PropertyName = "reactions")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking compat)
        public IList<MessageActionsPayloadReaction> Reactions { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets or sets the link back to the message.
        /// </summary>
        /// <value>The link back to the message.</value>
        [JsonProperty(PropertyName = "linkToMessage")]
        public Uri LinkToMessage { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
