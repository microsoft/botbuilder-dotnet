// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Adapters.Slack.Model;
using Microsoft.Bot.Schema;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Builder.Adapters.Slack.Tests
{
    public class SlackHelperTests
    {
        public const string ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQtB3AwMUeNoq4gUBGe6Ocj8kyh3bXa9ZbV7u1fVKQoyKFHdkqU";

        private readonly SlackClientWrapperOptions _testOptions = new SlackClientWrapperOptions("VerificationToken", "ClientSigningSecret", "BotToken");

        [Fact]
        public void ActivityToSlackShouldThrowArgumentNullExceptionWithNullActivity()
        {
            Assert.Throws<ArgumentNullException>(() => SlackHelper.ActivityToSlack(null));
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

            Assert.Equal(activity.Conversation.Id, message.Channel);
            Assert.Equal(activity.Attachments[0].Name, message.Attachments[0].AuthorName);
        }

        [Fact]
        public void ActivityToSlackShouldReturnMessageFromChannelData()
        {
            const string messageText = "Hello from message";

            var activity = new Activity
            {
                Timestamp = new DateTimeOffset(),
                Text = "Hello!",
                Recipient = new ChannelAccount("testRecipientId"),
                ChannelData = new NewSlackMessage
                {
                    Text = messageText,
                    Ephemeral = "testEphemeral"
                },
                Conversation = new ConversationAccount(id: "testId"),
            };

            var message = SlackHelper.ActivityToSlack(activity);

            Assert.Equal(messageText, message.Text);
        }

        [Fact]
        public void ActivityToSlackShouldReturnMessageWithThreadTs()
        {
            var serializeConversation = File.ReadAllText(Directory.GetCurrentDirectory() + @"/Files/SlackActivity.json");

            var activity = new Activity
            {
                Timestamp = new DateTimeOffset(),
                Text = "Hello!",
                Conversation = JsonConvert.DeserializeObject<ConversationAccount>(serializeConversation),
            };

            var message = SlackHelper.ActivityToSlack(activity);

            Assert.Equal(activity.Conversation.Id, message.Channel);
            Assert.Equal(activity.Conversation.Properties["thread_ts"].ToString(), message.ThreadTs);
        }

        [Fact]
        public async Task WriteAsyncShouldFailWithNullResponse()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await SlackHelper.WriteAsync(null, HttpStatusCode.OK, "testText", Encoding.UTF8, new CancellationToken()).ConfigureAwait(false);
            });
        }

        [Fact]
        public async Task WriteAsyncShouldFailWithNullText()
        {
            var httpResponse = new Mock<HttpResponse>();

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await SlackHelper.WriteAsync(httpResponse.Object, HttpStatusCode.OK, null, Encoding.UTF8, new CancellationToken()).ConfigureAwait(false);
            });
        }

        [Fact]
        public async Task WriteAsyncShouldFailWithNullEncoding()
        {
            var httpResponse = new Mock<HttpResponse>();

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await SlackHelper.WriteAsync(httpResponse.Object, HttpStatusCode.OK, "testText", null, new CancellationToken()).ConfigureAwait(false);
            });
        }

        [Fact]
        public void EventToActivityAsyncShouldThrowArgumentNullException()
        {
            var slackApi = new Mock<SlackClientWrapper>(_testOptions);

            Assert.Throws<ArgumentNullException>(() =>
            {
                SlackHelper.EventToActivity(null, slackApi.Object);
            });
        }

        [Fact]
        public void EventToActivityAsyncShouldReturnActivity()
        {
            var slackApi = new Mock<SlackClientWrapper>(_testOptions);

            var payload = File.ReadAllText(Directory.GetCurrentDirectory() + @"/Files/MessageBody.json");
            var slackBody = JsonConvert.DeserializeObject<EventRequest>(payload);

            var activity = SlackHelper.EventToActivity(slackBody, slackApi.Object);

            Assert.Equal(slackBody.Event.AdditionalProperties["text"].ToString(), activity.Text);
        }

        [Fact]
        public void EventToActivityAsyncShouldReturnActivityWithTeamId()
        {
            var slackApi = new Mock<SlackClientWrapper>(_testOptions);

            var payload = File.ReadAllText(Directory.GetCurrentDirectory() + @"/Files/MessageBody.json");
            var slackBody = JsonConvert.DeserializeObject<EventRequest>(payload);
            slackBody.Event.Channel = null;

            var activity = SlackHelper.EventToActivity(slackBody, slackApi.Object);

            Assert.Equal(slackBody.Event.AdditionalProperties["team"].ToString(), activity.Conversation.Id);
        }

        [Fact]
        public void CommandToActivityAsyncShouldThrowArgumentNullException()
        {
            var slackApi = new Mock<SlackClientWrapper>(_testOptions);

            Assert.Throws<ArgumentNullException>(() =>
            {
                SlackHelper.CommandToActivity(null, slackApi.Object);
            });
        }

        [Fact]
        public void CommandToActivityAsyncShouldReturnActivity()
        {
            var slackApi = new Mock<SlackClientWrapper>(_testOptions);

            var payload = File.ReadAllText(Directory.GetCurrentDirectory() + @"/Files/SlashCommandBody.txt");
            var commandBody = SlackHelper.QueryStringToDictionary(payload);
            var slackBody = JsonConvert.DeserializeObject<CommandPayload>(JsonConvert.SerializeObject(commandBody));

            var activity = SlackHelper.CommandToActivity(slackBody, slackApi.Object);

            Assert.Equal(slackBody.TriggerId, activity.Id);
        }

        [Fact]
        public void PayloadToActivityShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => SlackHelper.PayloadToActivity(null));
        }

        [Fact]
        public void QueryStringToDictionaryShouldReturnEmptyDictionary()
        {
            var dictionary = SlackHelper.QueryStringToDictionary(null);

            Assert.Empty(dictionary);
        }

        [Fact]
        public void QueryStringToDictionaryShouldReturnDictionary()
        {
            var payload = File.ReadAllText(Directory.GetCurrentDirectory() + @"/Files/SlashCommandBody.txt");

            var dictionary = SlackHelper.QueryStringToDictionary(payload);

            Assert.True(dictionary.Count > 0);
        }
    }
}
