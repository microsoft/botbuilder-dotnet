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
using Newtonsoft.Json.Linq;
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
            const string ephemeralValue = "testEphemeral";
            const string channelId = "channelId";
            const string userId = "testRecipientId";

            var activity = new Activity
            {
                Timestamp = new DateTimeOffset(),
                Text = "Hello!",
                Recipient = new ChannelAccount("testRecipientId"),
                ChannelData = new NewSlackMessage
                {
                    Text = messageText,
                    Ephemeral = ephemeralValue,
                    Channel = channelId,
                    User = userId,
                },
                Conversation = new ConversationAccount(id: "testId"),
            };

            var message = SlackHelper.ActivityToSlack(activity);

            Assert.Equal(messageText, message.Text);
            Assert.Equal(ephemeralValue, message.Ephemeral);
            Assert.Equal(channelId, message.Channel);
            Assert.Equal(userId, message.User);
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
        public void ActivityToSlackShouldConvertHeroCardsToBlocks()
        {
            var serializeConversation = File.ReadAllText(Directory.GetCurrentDirectory() + @"/Files/SlackActivity.json");

            var card = new HeroCard
            {
                Title = "BotFramework Hero Card",
                Subtitle = "Microsoft Bot Framework",
                Text = "Build and connect intelligent bots to interact with your users naturally wherever they are," +
                                " from text/sms to Skype, Slack, Office 365 mail and other popular services.",
                Images = new List<CardImage> { new CardImage("https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg") },
                Buttons = new List<CardAction>
                        {
                            new CardAction(ActionTypes.OpenUrl, "OpenUrl", value: "https://docs.microsoft.com/bot-framework"),
                            new CardAction
                            {
                                Text = "Some Action Text",
                                DisplayText = "ImBack",
                                Title = "Some Action Title",
                                Value = "Some Action Value",
                                Type = ActionTypes.ImBack
                            }
                        },
            };

            var activity = new Activity
            {
                Timestamp = new DateTimeOffset(),
                Text = "Hello!",
                Conversation = JsonConvert.DeserializeObject<ConversationAccount>(serializeConversation),
                Attachments = new List<Attachment>
                {
                    card.ToAttachment()
                }
            };

            var message = SlackHelper.ActivityToSlack(activity);

            Assert.Equal(activity.Conversation.Id, message.Channel);
            Assert.Equal(activity.Conversation.Properties["thread_ts"].ToString(), message.ThreadTs);
            Assert.Equal(card.Title, (message.Blocks as JArray)[0]["text"]["text"]);
            Assert.Equal(card.Subtitle, (message.Blocks as JArray)[1]["elements"][0]["text"]);
            Assert.Equal(card.Images[0].Url, (message.Blocks as JArray)[2]["image_url"]);
            Assert.Equal(card.Text, (message.Blocks as JArray)[3]["text"]["text"]);
            Assert.Equal(card.Buttons[0].Value, (message.Blocks as JArray)[5]["elements"][0]["url"].ToString());
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
        public void EventToActivityAsyncShouldReturnActivityWithAttachmentsWhenFileSharing()
        {
            var slackApi = new Mock<SlackClientWrapper>(_testOptions);

            var payload = File.ReadAllText(Directory.GetCurrentDirectory() + @"/Files/MessageBodyWithFileShare.json");
            var slackBody = JsonConvert.DeserializeObject<EventRequest>(payload);

            var activity = SlackHelper.EventToActivity(slackBody, slackApi.Object);

            Assert.Equal(ActivityTypes.Message, activity.Type);
            Assert.Equal(slackBody.Event.Type, activity.Type);
            Assert.Equal(slackBody.Event.AdditionalProperties["text"].ToString(), activity.Text);
            Assert.Equal(slackBody.Event.AdditionalProperties["files"][0]["mimetype"].ToString(), activity.Attachments[0].ContentType);
            Assert.Equal(slackBody.Event.AdditionalProperties["files"][0]["url_private_download"].ToString(), activity.Attachments[0].ContentUrl);
            Assert.Equal(slackBody.Event.AdditionalProperties["files"][0]["name"].ToString(), activity.Attachments[0].Name);
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
