// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Schema;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Builder.Adapters.Slack.Tests
{
    public class SlackHelperTests
    {
        public const string ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQtB3AwMUeNoq4gUBGe6Ocj8kyh3bXa9ZbV7u1fVKQoyKFHdkqU";

        [Fact]
        public void ActivityToSlackShouldReturnNullWithNullActivity()
        {
            Assert.Null(SlackHelper.ActivityToSlack(null));
        }

        [Fact]
        public void ActivityToSlackShouldReturnMessage()
        {
            var activity = new Activity
            {
                Timestamp = new DateTimeOffset(),
                Text = "Hello!",
                Attachments = new List<Attachment>
                {
                    new Attachment(name: "image", thumbnailUrl: ImageUrl),
                },
                Conversation = new ConversationAccount(id: "testId"),
            };

            var message = SlackHelper.ActivityToSlack(activity);

            Assert.Equal(activity.Conversation.Id, message.channel);
            Assert.Equal(activity.Attachments[0].Name, message.attachments[0].author_name);
        }

        [Fact]
        public void ActivityToSlackShouldReturnMessageFromChannelData()
        {
            var messageText = "Hello from message";

            var activity = new Activity
            {
                Timestamp = new DateTimeOffset(),
                Text = "Hello!",
                Recipient = new ChannelAccount("testRecipientId"),
                ChannelData = new NewSlackMessage
                {
                    text = messageText,
                    Ephemeral = "testEphimeral",
                    IconUrl = new Uri(ImageUrl),
                },
                Conversation = new ConversationAccount(id: "testId"),
            };

            var message = SlackHelper.ActivityToSlack(activity);

            Assert.Equal(messageText, message.text);
            Assert.False(message.AsUser);
        }

        [Fact]
        public void ActivityToSlackShouldReturnMessageWithThreadTS()
        {
            var serializeConversation = "{\"id\":\"testId\",\"thread_ts\":\"0001-01-01T00:00:00+00:00\"}";

            var activity = new Activity
            {
                Timestamp = new DateTimeOffset(),
                Text = "Hello!",
                Conversation = JsonConvert.DeserializeObject<ConversationAccount>(serializeConversation),
            };

            var message = SlackHelper.ActivityToSlack(activity);

            Assert.Equal(activity.Conversation.Id, message.channel);
            Assert.Equal(activity.Conversation.Properties["thread_ts"], message.thread_ts);
        }

        [Fact]
        public void VerifySignatureShouldReturnFalseWithNullParameters()
        {
            Assert.False(SlackHelper.VerifySignature(null, null, null));
        }

        [Fact]
        public void VerifySignatureShouldReturnTrue()
        {
            var body = File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\MessageBody.json");

            var httpRequest = new Mock<HttpRequest>();
            httpRequest.Setup(req => req.Headers.ContainsKey(It.IsAny<string>())).Returns(true);
            httpRequest.SetupGet(req => req.Headers["X-Slack-Request-Timestamp"]).Returns("0001-01-01T00:00:00+00:00");
            httpRequest.SetupGet(req => req.Headers["X-Slack-Signature"]).Returns("V0=389808EE538C31F2030C00A0A172BC75C349A39F84B86DBCE695706575FDA19B");

            Assert.True(SlackHelper.VerifySignature("secret", httpRequest.Object, body));
        }

        [Fact]
        public void GetMessageFromSlackEventShouldReturnNull()
        {
            Assert.Null(SlackHelper.GetMessageFromSlackEvent(null));
        }

        [Fact]
        public void GetMessageFromSlackEventShouldReturnMessage()
        {
            var json = File.ReadAllText(Directory.GetCurrentDirectory() + @"\Files\MessageBody.json");
            dynamic slackEvent = JsonConvert.DeserializeObject(json);

            var message = SlackHelper.GetMessageFromSlackEvent(slackEvent);

            Assert.Equal(slackEvent["event"].text.Value, message.text);
            Assert.Equal(slackEvent["event"].user.Value, message.user);
        }
    }
}
