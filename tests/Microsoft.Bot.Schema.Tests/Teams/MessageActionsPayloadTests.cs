// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    /// <summary>
    /// Tests to ensure that MessageActionsPayload works as expected.
    /// </summary>
    public class MessageActionsPayloadTests
    {
        private readonly string _id = "testId";
        private readonly string _replyId = "replyId";
        private readonly string _messageType = "message";
        private readonly bool _deleted = false;
        private readonly string _subject = "test-subject";
        private readonly string _summary = "test-summary";
        private readonly string _importance = "normal";
        private readonly string _locale = "en-us";
        private readonly string _attachmentLayout = "test-layout";
        private readonly string _date = DateTime.Today.ToString();
        private readonly MessageActionsPayloadFrom _from = new MessageActionsPayloadFrom();
        private readonly MessageActionsPayloadBody _body = new MessageActionsPayloadBody();
        private readonly List<MessageActionsPayloadAttachment> _attachments = new List<MessageActionsPayloadAttachment>();
        private readonly List<MessageActionsPayloadMention> _mentions = new List<MessageActionsPayloadMention>();
        private readonly List<MessageActionsPayloadReaction> _reactions = new List<MessageActionsPayloadReaction>();
        private readonly Uri _linkToMessage = new Uri("https://teams.microsoft.com/l/message/testing-id");

        /// <summary>
        /// Ensures the constructor of the <see cref="MessageActionsPayload"/> class works as expected.
        /// </summary>
        [Fact]
        public void TestMessageActionsPayloadConstructor()
        {
            var messageActionPayload = new MessageActionsPayload();

            Assert.Equal(default, messageActionPayload.Id);
            Assert.Equal(default, messageActionPayload.ReplyToId);
            Assert.Equal(default, messageActionPayload.MessageType);
            Assert.Equal(default, messageActionPayload.CreatedDateTime);
            Assert.Equal(default, messageActionPayload.LastModifiedDateTime);
            Assert.Equal(default, messageActionPayload.Deleted);
            Assert.Equal(default, messageActionPayload.Subject);
            Assert.Equal(default, messageActionPayload.Summary);
            Assert.Equal(default, messageActionPayload.Importance);
            Assert.Equal(default, messageActionPayload.Locale);
            Assert.Equal(default, messageActionPayload.From);
            Assert.Equal(default, messageActionPayload.Body);
            Assert.Equal(default, messageActionPayload.AttachmentLayout);
            Assert.Equal(default, messageActionPayload.Attachments);
            Assert.Equal(default, messageActionPayload.Mentions);
            Assert.Equal(default, messageActionPayload.Reactions);
        }

        /// <summary>
        /// Ensures the constructor of the <see cref="MessageActionsPayload"/> class works as expected with arguments.
        /// </summary>
        [Fact]
        public void TestMessageActionsPayloadConstructorWithArguments()
        {
            var payload = CreateActionPayload();

            Assert.Equal(_id, payload.Id);
            Assert.Equal(_replyId, payload.ReplyToId);
            Assert.Equal(_messageType, payload.MessageType);
            Assert.Equal(_date, payload.CreatedDateTime);
            Assert.Equal(_date, payload.LastModifiedDateTime);
            Assert.Equal(_deleted, payload.Deleted);
            Assert.Equal(_subject, payload.Subject);
            Assert.Equal(_summary, payload.Summary);
            Assert.Equal(_importance, payload.Importance);
            Assert.Equal(_locale, payload.Locale);
            Assert.Equal(_from, payload.From);
            Assert.Equal(_body, payload.Body);
            Assert.Equal(_attachmentLayout, payload.AttachmentLayout);
            Assert.Equal(_attachments, payload.Attachments);
            Assert.Equal(_mentions, payload.Mentions);
            Assert.Equal(_reactions, payload.Reactions);
        }

        /// <summary>
        /// Ensures that <see cref="MessageActionsPayload"/> class can be serialized and deserialized properly.
        /// </summary>
        [Fact]
        public void TestSerializationDeserialization()
        {
            var payload = CreateActionPayload();
            var serializedPayload = JsonConvert.SerializeObject(payload);
            var deserializedPayload = JsonConvert.DeserializeObject<MessageActionsPayload>(serializedPayload);

            Assert.Equal(payload.Id, deserializedPayload.Id);
            Assert.Equal(payload.ReplyToId, deserializedPayload.ReplyToId);
            Assert.Equal(payload.MessageType, deserializedPayload.MessageType);
            Assert.Equal(payload.CreatedDateTime, deserializedPayload.CreatedDateTime);
            Assert.Equal(payload.LastModifiedDateTime, deserializedPayload.LastModifiedDateTime);
            Assert.Equal(payload.Deleted, deserializedPayload.Deleted);
            Assert.Equal(payload.Subject, deserializedPayload.Subject);
            Assert.Equal(payload.Summary, deserializedPayload.Summary);
            Assert.Equal(payload.Importance, deserializedPayload.Importance);
            Assert.Equal(payload.Locale, deserializedPayload.Locale);
            Assert.Equal(payload.From.User, deserializedPayload.From.User);
            Assert.Equal(payload.Body.Content, deserializedPayload.Body.Content);
            Assert.Equal(payload.AttachmentLayout, deserializedPayload.AttachmentLayout);
            Assert.Equal(payload.Attachments, deserializedPayload.Attachments);
            Assert.Equal(payload.Mentions, deserializedPayload.Mentions);
            Assert.Equal(payload.Reactions, deserializedPayload.Reactions);
            Assert.Equal(payload.LinkToMessage, deserializedPayload.LinkToMessage);
        }

        /// <summary>
        /// Creates a <see cref="MessageActionsPayload"/> to be used in the tests.
        /// </summary>
        /// <returns>A MessageActionsPayload set with testing values.</returns>
        private MessageActionsPayload CreateActionPayload()
        {
            return new MessageActionsPayload
            {
                Id = _id,
                ReplyToId = _replyId,
                MessageType = _messageType,
                CreatedDateTime = _date,
                LastModifiedDateTime = _date,
                Deleted = _deleted,
                Subject = _subject,
                Summary = _summary,
                Importance = _importance,
                Locale = _locale,
                From = _from,
                Body = _body,
                AttachmentLayout = _attachmentLayout,
                Attachments = _attachments,
                Mentions = _mentions,
                Reactions = _reactions,
                LinkToMessage = _linkToMessage
            };
        }
    }
}
