// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    /// <summary>
    /// Tests to ensure that MessageActionsPayload works as expected.
    /// </summary>
    public class MessageActionsPayloadTests
    {
        /// <summary>
        /// Ensures the constructor of the <see cref="MessageActionsPayload"/> class works as expected.
        /// </summary>
        [Fact]
        public void TestMessageActionsPayloadConstructor()
        {
            var messageActionPayload = new MessageActionsPayload();

            Assert.NotNull(messageActionPayload);
        }

        /// <summary>
        /// Ensures the constructor of the <see cref="MessageActionsPayload"/> class works as expected with arguments.
        /// </summary>
        [Fact]
        public void TestMessageActionsPayloadConstructorWithArguments()
        {
            var messageActionPayload = new MessageActionsPayload
            {
                Id = "testId",
                CreatedDateTime = DateTime.Today.ToString(),
                Deleted = false,
                Importance = "normal",
                Locale = "en-us",
                From = new MessageActionsPayloadFrom(),
                Body = new MessageActionsPayloadBody(),
                Attachments = new List<MessageActionsPayloadAttachment>(),
                Mentions = new List<MessageActionsPayloadMention>(),
                LinkToMessage = new Uri("https://teams.microsoft.com/l/message/testing-id")
            };

            Assert.NotNull(messageActionPayload);
        }

        /// <summary>
        /// Ensures that the Id property can be set and retrieved.
        /// </summary>
        [Fact]
        public void TestGetId()
        {
            var id = "testId";
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "composeExtension/submitAction",
                ChannelData = new MessagingExtensionAction { MessagePayload = new MessageActionsPayload { Id = id } }
            };

            var channelData = activity.GetChannelData<MessagingExtensionAction>();

            Assert.Equal(id, channelData.MessagePayload.Id);
        }

        /// <summary>
        /// Ensures that the ReplyToId property can be set and retrieved.
        /// </summary>
        [Fact]
        public void TestGetReplyToId()
        {
            var replyToId = "replyId";
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "composeExtension/submitAction",
                ChannelData = new MessagingExtensionAction { MessagePayload = new MessageActionsPayload { ReplyToId = replyToId } }
            };

            var channelData = activity.GetChannelData<MessagingExtensionAction>();

            Assert.Equal(replyToId, channelData.MessagePayload.ReplyToId);
        }

        /// <summary>
        /// Ensures that the MessageType property can be set and retrieved.
        /// </summary>
        [Fact]
        public void TestGetMessageType()
        {
            var messageType = "message";
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "composeExtension/submitAction",
                ChannelData = new MessagingExtensionAction { MessagePayload = new MessageActionsPayload { MessageType = messageType } }
            };

            var channelData = activity.GetChannelData<MessagingExtensionAction>();

            Assert.Equal(messageType, channelData.MessagePayload.MessageType);
        }

        /// <summary>
        /// Ensures that the CreatedDateTime property can be set and retrieved.
        /// </summary>
        [Fact]
        public void TestGetCreatedDateTime()
        {
            var createdDateTime = DateTime.Today.ToString();
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "composeExtension/submitAction",
                ChannelData = new MessagingExtensionAction { MessagePayload = new MessageActionsPayload { CreatedDateTime = createdDateTime } }
            };

            var channelData = activity.GetChannelData<MessagingExtensionAction>();

            Assert.Equal(createdDateTime, channelData.MessagePayload.CreatedDateTime);
        }

        /// <summary>
        /// Ensures that the LastModifiedDateTime property can be set and retrieved.
        /// </summary>
        [Fact]
        public void TestGetLastModifiedDateTime()
        {
            var lastModifiedDateTime = DateTime.Today.ToString();
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "composeExtension/submitAction",
                ChannelData = new MessagingExtensionAction { MessagePayload = new MessageActionsPayload { LastModifiedDateTime = lastModifiedDateTime } }
            };

            var channelData = activity.GetChannelData<MessagingExtensionAction>();

            Assert.Equal(lastModifiedDateTime, channelData.MessagePayload.LastModifiedDateTime);
        }

        /// <summary>
        /// Ensures that the Deleted property can be set and retrieved.
        /// </summary>
        [Fact]
        public void TestGetDeleted()
        {
            var deleted = true;
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "composeExtension/submitAction",
                ChannelData = new MessagingExtensionAction { MessagePayload = new MessageActionsPayload { Deleted = deleted } }
            };

            var channelData = activity.GetChannelData<MessagingExtensionAction>();

            Assert.True(channelData.MessagePayload.Deleted);
        }

        /// <summary>
        /// Ensures that the Subject property can be set and retrieved.
        /// </summary>
        [Fact]
        public void TestGetSubject()
        {
            var subject = "test subject";
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "composeExtension/submitAction",
                ChannelData = new MessagingExtensionAction { MessagePayload = new MessageActionsPayload { Subject = subject } }
            };

            var channelData = activity.GetChannelData<MessagingExtensionAction>();

            Assert.Equal(subject, channelData.MessagePayload.Subject);
        }

        /// <summary>
        /// Ensures that the Summary property can be set and retrieved.
        /// </summary>
        [Fact]
        public void TestGetSummary()
        {
            var summary = "test summary";
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "composeExtension/submitAction",
                ChannelData = new MessagingExtensionAction { MessagePayload = new MessageActionsPayload { Summary = summary } }
            };

            var channelData = activity.GetChannelData<MessagingExtensionAction>();

            Assert.Equal(summary, channelData.MessagePayload.Summary);
        }

        /// <summary>
        /// Ensures that the Importance property can be set and retrieved.
        /// </summary>
        [Fact]
        public void TestGetImportance()
        {
            var importance = "normal";
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "composeExtension/submitAction",
                ChannelData = new MessagingExtensionAction { MessagePayload = new MessageActionsPayload { Importance = importance } }
            };

            var channelData = activity.GetChannelData<MessagingExtensionAction>();

            Assert.Equal(importance, channelData.MessagePayload.Importance);
        }

        /// <summary>
        /// Ensures that the Locale property can be set and retrieved.
        /// </summary>
        [Fact]
        public void TestGetLocale()
        {
            var locale = "en-us";
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "composeExtension/submitAction",
                ChannelData = new MessagingExtensionAction { MessagePayload = new MessageActionsPayload { Locale = locale } }
            };

            var channelData = activity.GetChannelData<MessagingExtensionAction>();

            Assert.Equal(locale, channelData.MessagePayload.Locale);
        }

        /// <summary>
        /// Ensures that the From property can be set and retrieved.
        /// </summary>
        [Fact]
        public void TestGetFrom()
        {
            var from = new MessageActionsPayloadFrom { User = new MessageActionsPayloadUser("testUser") };
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "composeExtension/submitAction",
                ChannelData = new MessagingExtensionAction { MessagePayload = new MessageActionsPayload { From = from } }
            };

            var channelData = activity.GetChannelData<MessagingExtensionAction>();

            Assert.Equal(from, channelData.MessagePayload.From);
        }

        /// <summary>
        /// Ensures that the Body property can be set and retrieved.
        /// </summary>
        [Fact]
        public void TestGetBody()
        {
            var body = new MessageActionsPayloadBody { ContentType = "text", Content = "test body" };
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "composeExtension/submitAction",
                ChannelData = new MessagingExtensionAction { MessagePayload = new MessageActionsPayload { Body = body } }
            };

            var channelData = activity.GetChannelData<MessagingExtensionAction>();

            Assert.Equal(body, channelData.MessagePayload.Body);
        }

        /// <summary>
        /// Ensures that the AttachmentLayout property can be set and retrieved.
        /// </summary>
        [Fact]
        public void TestGetAttachmentLayout()
        {
            var attachmentLayout = "testLayout";
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "composeExtension/submitAction",
                ChannelData = new MessagingExtensionAction { MessagePayload = new MessageActionsPayload { AttachmentLayout = attachmentLayout } }
            };

            var channelData = activity.GetChannelData<MessagingExtensionAction>();

            Assert.Equal(attachmentLayout, channelData.MessagePayload.AttachmentLayout);
        }

        /// <summary>
        /// Ensures that the Attachments property can be set and retrieved.
        /// </summary>
        [Fact]
        public void TestGetAttachments()
        {
            var attachments = new List<MessageActionsPayloadAttachment>
            {
                new MessageActionsPayloadAttachment { Id = "attachment1" },
                new MessageActionsPayloadAttachment { Id = "attachment2" }
            };

            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "composeExtension/submitAction",
                ChannelData = new MessagingExtensionAction { MessagePayload = new MessageActionsPayload { Attachments = attachments } }
            };

            var channelData = activity.GetChannelData<MessagingExtensionAction>();

            Assert.Equal(attachments, channelData.MessagePayload.Attachments);
        }

        /// <summary>
        /// Ensures that the Mentions property can be set and retrieved.
        /// </summary>
        [Fact]
        public void TestGetMentions()
        {
            var mentions = new List<MessageActionsPayloadMention> 
            { 
                new MessageActionsPayloadMention { Id = 1 },
                new MessageActionsPayloadMention { Id = 2 }
            };

            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "composeExtension/submitAction",
                ChannelData = new MessagingExtensionAction { MessagePayload = new MessageActionsPayload { Mentions = mentions } }
            };

            var channelData = activity.GetChannelData<MessagingExtensionAction>();

            Assert.Equal(mentions, channelData.MessagePayload.Mentions);
        }

        /// <summary>
        /// Ensures that the Reactions property can be set and retrieved.
        /// </summary>
        [Fact]
        public void TestGetReactions()
        {
            var reactions = new List<MessageActionsPayloadReaction>
            {
                new MessageActionsPayloadReaction { ReactionType = "like" },
                new MessageActionsPayloadReaction { ReactionType = "heart" }
            };

            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "composeExtension/submitAction",
                ChannelData = new MessagingExtensionAction { MessagePayload = new MessageActionsPayload { Reactions = reactions } }
            };

            var channelData = activity.GetChannelData<MessagingExtensionAction>();

            Assert.Equal(reactions, channelData.MessagePayload.Reactions);
        }

        /// <summary>
        /// Ensures that the LinkToMessage property can be set and retrieved.
        /// </summary>
        [Fact]
        public void TestGetLinkToMessage()
        {
            var linkToMessage = new Uri("https://teams.microsoft.com/l/message/testing-id");
            var activity = new Activity 
            {
                Type = ActivityTypes.Invoke,
                Name = "composeExtension/submitAction",
                ChannelData = new MessagingExtensionAction { MessagePayload = new MessageActionsPayload { LinkToMessage = linkToMessage } }
            };

            var channelData = activity.GetChannelData<MessagingExtensionAction>();

            Assert.Equal(linkToMessage, channelData.MessagePayload.LinkToMessage);
        }
    }
}
