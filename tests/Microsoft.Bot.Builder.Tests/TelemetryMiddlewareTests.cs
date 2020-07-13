// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Builder.Tests
{
    public class TelemetryMiddlewareTests
    {
        [Fact]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task Telemetry_NullTelemetryClient()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            // Arrange / Act
            //
            // Note: The TelemetryClient will most likely be DI'd in, and handling a null
            // TelemetryClient should be handled by placing the NullBotTelemetryClient().
            var logger = new TelemetryLoggerMiddleware(null, logPersonalInformation: true);

            // Assert
            Assert.NotNull(logger);
        }

        [Fact]
        public async Task Telemetry_LogActivities()
        {
            // Arrange
            var mockTelemetryClient = new Mock<IBotTelemetryClient>();
            TestAdapter adapter = new TestAdapter()
                .Use(new TelemetryLoggerMiddleware(mockTelemetryClient.Object, logPersonalInformation: true));
            string conversationId = null;

            // Act
            // Default case logging Send/Receive Activities
            await new TestFlow(adapter, async (context, cancellationToken) =>
            {
                conversationId = context.Activity.Conversation.Id;
                var typingActivity = new Activity
                {
                    Type = ActivityTypes.Typing,
                    RelatesTo = context.Activity.RelatesTo,
                };
                await context.SendActivityAsync(typingActivity);
                await Task.Delay(500);
                await context.SendActivityAsync("echo:" + context.Activity.Text);
            })
                .Send("foo")
                    .AssertReply((activity) => Assert.Equal(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:foo")
                .Send("bar")
                    .AssertReply((activity) => Assert.Equal(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:bar")
                .StartTestAsync();

            // Assert
            Assert.Equal(6, mockTelemetryClient.Invocations.Count);
            Assert.Equal("BotMessageReceived", mockTelemetryClient.Invocations[0].Arguments[0]); // Check initial message
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).Count == 7);
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("fromId"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("conversationName"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("locale"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("recipientId"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("recipientName"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("fromName"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("text"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1])["text"] == "foo");

            Assert.Equal("BotMessageSend", mockTelemetryClient.Invocations[1].Arguments[0]); // Check Typing message
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).Count == 5);
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("replyActivityId"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("recipientId"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("conversationName"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("locale"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("recipientName"));

            Assert.Equal("BotMessageSend", mockTelemetryClient.Invocations[2].Arguments[0]); // Check message reply
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).Count == 6);
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("replyActivityId"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("recipientId"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("conversationName"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("locale"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("recipientName"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("text"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1])["text"] == "echo:foo");

            Assert.Equal("BotMessageReceived", mockTelemetryClient.Invocations[3].Arguments[0]); // Check bar message
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).Count == 7);
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("fromId"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("conversationName"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("locale"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("recipientId"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("recipientName"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("fromName"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("text"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1])["text"] == "bar");
        }

        [Fact]
        public async Task Telemetry_NoPII()
        {
            // Arrange
            var mockTelemetryClient = new Mock<IBotTelemetryClient>();
            TestAdapter adapter = new TestAdapter()
                .Use(new TelemetryLoggerMiddleware(mockTelemetryClient.Object, logPersonalInformation: false));
            string conversationId = null;

            // Act
            // Ensure LogPersonalInformation flag works
            await new TestFlow(adapter, async (context, cancellationToken) =>
            {
                conversationId = context.Activity.Conversation.Id;
                var typingActivity = new Activity
                {
                    Type = ActivityTypes.Typing,
                    RelatesTo = context.Activity.RelatesTo,
                };
                await context.SendActivityAsync(typingActivity);
                await Task.Delay(500);
                await context.SendActivityAsync("echo:" + context.Activity.Text);
            })
                .Send("foo")
                    .AssertReply((activity) => Assert.Equal(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:foo")
                .Send("bar")
                    .AssertReply((activity) => Assert.Equal(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:bar")
                .StartTestAsync();

            // Assert
            Assert.Equal(6, mockTelemetryClient.Invocations.Count);
            Assert.Equal("BotMessageReceived", mockTelemetryClient.Invocations[0].Arguments[0]); // Check initial message
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).Count == 5);
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("fromId"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("conversationName"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("locale"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("recipientId"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("recipientName"));
            Assert.False(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("fromName"));
            Assert.False(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("text"));

            Assert.Equal("BotMessageSend", mockTelemetryClient.Invocations[1].Arguments[0]); // Check Typing message
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).Count == 4);
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("replyActivityId"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("recipientId"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("conversationName"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("locale"));
            Assert.False(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("recipientName"));

            Assert.Equal("BotMessageSend", mockTelemetryClient.Invocations[2].Arguments[0]); // Check message reply
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).Count == 4);
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("replyActivityId"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("recipientId"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("conversationName"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("locale"));
            Assert.False(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("recipientName"));
            Assert.False(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("text"));

            Assert.Equal("BotMessageReceived", mockTelemetryClient.Invocations[3].Arguments[0]); // Check bar message
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).Count == 5);
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("fromId"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("conversationName"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("locale"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("recipientId"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("recipientName"));
            Assert.False(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("fromName"));
            Assert.False(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("text"));
        }

        [Fact]
        public async Task Transcript_LogUpdateActivities()
        {
            // Arrange
            var mockTelemetryClient = new Mock<IBotTelemetryClient>();
            TestAdapter adapter = new TestAdapter()
                .Use(new TelemetryLoggerMiddleware(mockTelemetryClient.Object, logPersonalInformation: true));
            string conversationId = null;
            Activity activityToUpdate = null;

            // Act
            // Test Update Activities
            await new TestFlow(adapter, async (context, cancellationToken) =>
            {
                conversationId = context.Activity.Conversation.Id;
                if (context.Activity.Text == "update")
                {
                    activityToUpdate.Text = "new response";
                    await context.UpdateActivityAsync(activityToUpdate);
                }
                else
                {
                    var activity = context.Activity.CreateReply("response");
                    var response = await context.SendActivityAsync(activity);
                    activity.Id = response.Id;

                    // clone the activity, so we can use it to do an update
                    activityToUpdate = JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject(activity));
                }
            })
                .Send("foo")
                .Send("update")
                    .AssertReply("new response")
                .StartTestAsync();

            // Assert
            Assert.Equal(4, mockTelemetryClient.Invocations.Count);
            Assert.Equal("BotMessageUpdate", mockTelemetryClient.Invocations[3].Arguments[0]); // Check update message
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).Count == 5);
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("recipientId"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("conversationId"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("conversationName"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("locale"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("text"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1])["text"] == "new response");
        }

        [Fact]
        public async Task Transcript_LogDeleteActivities()
        {
            // Arrange
            var mockTelemetryClient = new Mock<IBotTelemetryClient>();
            TestAdapter adapter = new TestAdapter()
                .Use(new TelemetryLoggerMiddleware(mockTelemetryClient.Object, logPersonalInformation: true));
            string conversationId = null;
            string activityId = null;

            // Act
            // Verify Delete Activities are logged.
            await new TestFlow(adapter, async (context, cancellationToken) =>
            {
                conversationId = context.Activity.Conversation.Id;
                if (context.Activity.Text == "deleteIt")
                {
                    await context.DeleteActivityAsync(activityId);
                }
                else
                {
                    var activity = context.Activity.CreateReply("response");
                    var response = await context.SendActivityAsync(activity);
                    activityId = response.Id;
                }
            })
                .Send("foo")
                    .AssertReply("response")
                .Send("deleteIt")
                .StartTestAsync();

            // Assert
            Assert.Equal(4, mockTelemetryClient.Invocations.Count);
            Assert.Equal("BotMessageDelete", mockTelemetryClient.Invocations[3].Arguments[0]); // Check delete message
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).Count == 3);
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("recipientId"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("conversationId"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("conversationName"));
        }

        [Fact]
        public async Task Telemetry_OverrideReceive()
        {
            // Arrange
            var mockTelemetryClient = new Mock<IBotTelemetryClient>();
            TestAdapter adapter = new TestAdapter()
                .Use(new OverrideReceiveLogger(mockTelemetryClient.Object, logPersonalInformation: true));
            string conversationId = null;

            // Act
            // Override the TelemetryMiddleware component and override the Receive event.
            await new TestFlow(adapter, async (context, cancellationToken) =>
            {
                conversationId = context.Activity.Conversation.Id;
                var typingActivity = new Activity
                {
                    Type = ActivityTypes.Typing,
                    RelatesTo = context.Activity.RelatesTo,
                };
                await context.SendActivityAsync(typingActivity);
                await Task.Delay(500);
                await context.SendActivityAsync("echo:" + context.Activity.Text);
            })
                .Send("foo")
                    .AssertReply((activity) => Assert.Equal(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:foo")
                .Send("bar")
                    .AssertReply((activity) => Assert.Equal(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:bar")
                .StartTestAsync();

            // Assert
            Assert.Equal(8, mockTelemetryClient.Invocations.Count);
            Assert.Equal(mockTelemetryClient.Invocations[0].Arguments[0], TelemetryLoggerConstants.BotMsgReceiveEvent); // Check initial message
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).Count == 2);
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("foo"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1])["foo"] == "bar");
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("ImportantProperty"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1])["ImportantProperty"] == "ImportantValue");

            Assert.Equal("MyReceive", mockTelemetryClient.Invocations[1].Arguments[0]); // Check my message
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("fromId"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("conversationName"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("locale"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("recipientId"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("recipientName"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("fromName"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("text"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1])["text"] == "foo");

            Assert.Equal("BotMessageSend", mockTelemetryClient.Invocations[2].Arguments[0]); // Check Typing message
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).Count == 5);
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("replyActivityId"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("recipientId"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("conversationName"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("locale"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("recipientName"));

            Assert.Equal("BotMessageSend", mockTelemetryClient.Invocations[3].Arguments[0]); // Check message reply
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).Count == 6);
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("replyActivityId"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("recipientId"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("conversationName"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("locale"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("recipientName"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("text"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1])["text"] == "echo:foo");
        }

        [Fact]
        public async Task Telemetry_OverrideSend()
        {
            // Arrange
            var mockTelemetryClient = new Mock<IBotTelemetryClient>();
            TestAdapter adapter = new TestAdapter()
                .Use(new OverrideSendLogger(mockTelemetryClient.Object, logPersonalInformation: true));
            string conversationId = null;

            // Act
            // Override the TelemetryMiddleware component and override the Send event.
            await new TestFlow(adapter, async (context, cancellationToken) =>
            {
                conversationId = context.Activity.Conversation.Id;
                var typingActivity = new Activity
                {
                    Type = ActivityTypes.Typing,
                    RelatesTo = context.Activity.RelatesTo,
                };
                await context.SendActivityAsync(typingActivity);
                await Task.Delay(500);
                await context.SendActivityAsync("echo:" + context.Activity.Text);
            })
                .Send("foo")
                    .AssertReply((activity) => Assert.Equal(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:foo")
                .Send("bar")
                    .AssertReply((activity) => Assert.Equal(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:bar")
                .StartTestAsync();

            Assert.Equal(10, mockTelemetryClient.Invocations.Count);
            Assert.Equal(mockTelemetryClient.Invocations[0].Arguments[0], TelemetryLoggerConstants.BotMsgReceiveEvent); // Check initial message
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).Count == 7);
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("fromId"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("conversationName"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("locale"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("recipientId"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("recipientName"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("fromName"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("text"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1])["text"] == "foo");

            Assert.Equal(mockTelemetryClient.Invocations[1].Arguments[0], TelemetryLoggerConstants.BotMsgSendEvent); // Check Typing message (1 of 2)
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).Count == 2);
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("foo"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1])["foo"] == "bar");
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("ImportantProperty"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1])["ImportantProperty"] == "ImportantValue");

            Assert.Equal("MySend", mockTelemetryClient.Invocations[2].Arguments[0]); // Check custom message
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).Count == 5);
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("replyActivityId"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("recipientId"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("conversationName"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("locale"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("recipientName"));
        }

        [Fact]
        public async Task Telemetry_OverrideUpdateDeleteActivities()
        {
            var mockTelemetryClient = new Mock<IBotTelemetryClient>();
            TestAdapter adapter = new TestAdapter()
                .Use(new OverrideUpdateDeleteLogger(mockTelemetryClient.Object, logPersonalInformation: true));
            string conversationId = null;
            Activity activityToUpdate = null;
            await new TestFlow(adapter, async (context, cancellationToken) =>
            {
                conversationId = context.Activity.Conversation.Id;
                if (context.Activity.Text == "update")
                {
                    activityToUpdate.Text = "new response";
                    await context.UpdateActivityAsync(activityToUpdate);
                    await context.DeleteActivityAsync(context.Activity.Id);
                }
                else
                {
                    var activity = context.Activity.CreateReply("response");
                    var response = await context.SendActivityAsync(activity);
                    activity.Id = response.Id;

                    // clone the activity, so we can use it to do an update
                    activityToUpdate = JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject(activity));
                }
            })
                .Send("foo")
                .Send("update")
                    .AssertReply("new response")
                .StartTestAsync();

            Assert.Equal(5, mockTelemetryClient.Invocations.Count);
            Assert.Equal(mockTelemetryClient.Invocations[3].Arguments[0], TelemetryLoggerConstants.BotMsgUpdateEvent); // Check update message
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).Count == 2);
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("foo"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1])["foo"] == "bar");
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("ImportantProperty"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1])["ImportantProperty"] == "ImportantValue");

            Assert.Equal(mockTelemetryClient.Invocations[4].Arguments[0], TelemetryLoggerConstants.BotMsgDeleteEvent); // Check delete message
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[4].Arguments[1]).Count == 2);
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[4].Arguments[1]).ContainsKey("foo"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[4].Arguments[1])["foo"] == "bar");
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[4].Arguments[1]).ContainsKey("ImportantProperty"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[4].Arguments[1])["ImportantProperty"] == "ImportantValue");
        }

        [Fact]
        public async Task Telemetry_AdditionalProps()
        {
            var mockTelemetryClient = new Mock<IBotTelemetryClient>();
            TestAdapter adapter = new TestAdapter()
                .Use(new OverrideFillLogger(mockTelemetryClient.Object, logPersonalInformation: true));
            string conversationId = null;
            Activity activityToUpdate = null;
            await new TestFlow(adapter, async (context, cancellationToken) =>
            {
                conversationId = context.Activity.Conversation.Id;
                if (context.Activity.Text == "update")
                {
                    activityToUpdate.Text = "new response";

                    // Perform Update Delete
                    await context.UpdateActivityAsync(activityToUpdate);
                    await context.DeleteActivityAsync(context.Activity.Id);
                }
                else
                {
                    // Perform Send/Receive
                    var activity = context.Activity.CreateReply("response");
                    var response = await context.SendActivityAsync(activity);
                    activity.Id = response.Id;

                    // clone the activity, so we can use it to do an update
                    activityToUpdate = JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject(activity));
                }
            })
                .Send("foo")
                .Send("update")
                    .AssertReply("new response")
                .StartTestAsync();

            Assert.Equal(mockTelemetryClient.Invocations[0].Arguments[0], TelemetryLoggerConstants.BotMsgReceiveEvent); // Check Receive message
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("fromId"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("conversationName"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("locale"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("recipientId"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("recipientName"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("fromName"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("text"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1])["text"] == "foo");

            Assert.Equal(mockTelemetryClient.Invocations[1].Arguments[0], TelemetryLoggerConstants.BotMsgSendEvent); // Check Send message
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).Count == 8);
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("replyActivityId"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("recipientId"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("conversationName"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("locale"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("recipientName"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("foo"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("text"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1])["text"] == "response");
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1])["foo"] == "bar");
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1])["ImportantProperty"] == "ImportantValue");

            Assert.Equal(mockTelemetryClient.Invocations[3].Arguments[0], TelemetryLoggerConstants.BotMsgUpdateEvent); // Check Update message
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).Count == 7);
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("conversationId"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("conversationName"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("locale"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("foo"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("text"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1])["text"] == "new response");
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1])["foo"] == "bar");
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1])["ImportantProperty"] == "ImportantValue");

            Assert.Equal(mockTelemetryClient.Invocations[4].Arguments[0], TelemetryLoggerConstants.BotMsgDeleteEvent); // Check Delete message
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[4].Arguments[1]).Count == 5);
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[4].Arguments[1]).ContainsKey("recipientId"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[4].Arguments[1]).ContainsKey("conversationName"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[4].Arguments[1]).ContainsKey("conversationId"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[4].Arguments[1]).ContainsKey("foo"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[4].Arguments[1])["foo"] == "bar");
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[4].Arguments[1])["ImportantProperty"] == "ImportantValue");
        }

        [Fact]
        public async Task DoNotThrowOnNullFrom()
        {
            // Arrange
            var mockTelemetryClient = new Mock<IBotTelemetryClient>();
            TestAdapter adapter = new TestAdapter(new ConversationReference() 
            { 
                ChannelId = Channels.Test,
                Bot = new ChannelAccount("bot", "Bot"),
                Conversation = new ConversationAccount() { Id = Guid.NewGuid().ToString("n") }
            })
                .Use(new TelemetryLoggerMiddleware(mockTelemetryClient.Object, logPersonalInformation: false));
            
            // Act
            // Ensure LogPersonalInformation flag works
            await new TestFlow(adapter, async (context, cancellationToken) =>
            {
                await adapter.CreateConversationAsync(
                    context.Activity.ChannelId, 
                    async (context, cancellationToken) =>
                    {
                        await context.SendActivityAsync("proactive");
                    }, 
                    new CancellationToken());
            })
                .Send("foo")
                    .AssertReply("proactive")
                .StartTestAsync();

            // Assert
            Assert.Equal(1, mockTelemetryClient.Invocations.Count);
            Assert.Equal("BotMessageReceived", mockTelemetryClient.Invocations[0].Arguments[0]); // Check initial message
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).Count == 5);
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("fromId"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1])["fromId"] == null);
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("conversationName"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("locale"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("recipientId"));
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("recipientName"));
            Assert.False(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("fromName"));
            Assert.False(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("text"));
        }

        [Fact]
        public async Task Telemetry_LogTeamsProperties()
        {
            // Arrange
            var mockTelemetryClient = new Mock<IBotTelemetryClient>();

            var adapter = new TestAdapter(Channels.Msteams)
                .Use(new TelemetryLoggerMiddleware(mockTelemetryClient.Object));

            var teamInfo = new TeamInfo("teamId", "teamName");
            var channelData = new TeamsChannelData(null, null, teamInfo, null, new TenantInfo("tenantId"));
            var activity = MessageFactory.Text("test");

            activity.ChannelData = channelData;
            activity.From = new ChannelAccount("userId", "userName", null, "aadId");

            // Act
            await new TestFlow(adapter)
                .Send(activity)
                .StartTestAsync();

            // Assert
            Assert.Equal("BotMessageReceived", mockTelemetryClient.Invocations[0].Arguments[0]);
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1])["TeamsUserAadObjectId"] == "aadId");
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1])["TeamsTenantId"] == "tenantId");
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1])["TeamsTeamInfo"] == JsonConvert.SerializeObject(teamInfo));
        }
      
        public async Task DoNotThrowOnNullActivity()
        {
            // Arrange
            var mockTelemetryClient = new Mock<IBotTelemetryClient>();
            
            var middleware = new OverriddenOnTurnLogger(mockTelemetryClient.Object, true);

            await middleware.OnTurnAsync(null, null, CancellationToken.None);

            // Assert
            Assert.Equal(4, mockTelemetryClient.Invocations.Count);
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).Count == 0);
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).Count == 0);
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).Count == 0);
            Assert.True(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).Count == 0);
        }

        public class OverriddenOnTurnLogger : TelemetryLoggerMiddleware
        {
            public OverriddenOnTurnLogger(IBotTelemetryClient telemetryClient, bool logPersonalInformation = false)
                : base(telemetryClient, logPersonalInformation)
            {
            }

            public override async Task OnTurnAsync(ITurnContext context, NextDelegate nextTurn, CancellationToken cancellationToken)
            {
                await OnReceiveActivityAsync(null, cancellationToken);
                await OnUpdateActivityAsync(null, cancellationToken);
                await OnSendActivityAsync(null, cancellationToken);
                await OnDeleteActivityAsync(null, cancellationToken);
            }
        }

        public class OverrideReceiveLogger : TelemetryLoggerMiddleware
        {
            public OverrideReceiveLogger(IBotTelemetryClient telemetryClient, bool logPersonalInformation = false)
                : base(telemetryClient, logPersonalInformation)
            {
            }

            protected override async Task OnReceiveActivityAsync(Activity activity, CancellationToken cancellation)
            {
                var properties = new Dictionary<string, string>
                {
                    { "foo", "bar" },
                    { "ImportantProperty", "ImportantValue" },
                };
                TelemetryClient.TrackEvent(TelemetryLoggerConstants.BotMsgReceiveEvent, properties);

                TelemetryClient.TrackEvent("MyReceive", await FillReceiveEventPropertiesAsync(activity).ConfigureAwait(false));
                return;
            }
        }

        public class OverrideSendLogger : TelemetryLoggerMiddleware
        {
            public OverrideSendLogger(IBotTelemetryClient telemetryClient, bool logPersonalInformation = false)
                : base(telemetryClient, logPersonalInformation)
            {
            }

            protected override async Task OnSendActivityAsync(Activity activity, CancellationToken cancellation)
            {
                var properties = new Dictionary<string, string>
                {
                    { "foo", "bar" },
                    { "ImportantProperty", "ImportantValue" },
                };
                TelemetryClient.TrackEvent(TelemetryLoggerConstants.BotMsgSendEvent, properties);

                TelemetryClient.TrackEvent("MySend", await FillSendEventPropertiesAsync(activity).ConfigureAwait(false));
                return;
            }
        }

        public class OverrideUpdateDeleteLogger : TelemetryLoggerMiddleware
        {
            public OverrideUpdateDeleteLogger(IBotTelemetryClient telemetryClient, bool logPersonalInformation = false)
                : base(telemetryClient, logPersonalInformation)
            {
            }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            protected override async Task OnUpdateActivityAsync(Activity activity, CancellationToken cancellation)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            {
                var properties = new Dictionary<string, string>
                {
                    { "foo", "bar" },
                    { "ImportantProperty", "ImportantValue" },
                };
                TelemetryClient.TrackEvent(TelemetryLoggerConstants.BotMsgUpdateEvent, properties);
                return;
            }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            protected override async Task OnDeleteActivityAsync(Activity activity, CancellationToken cancellation)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            {
                var properties = new Dictionary<string, string>
                {
                    { "foo", "bar" },
                    { "ImportantProperty", "ImportantValue" },
                };
                TelemetryClient.TrackEvent(TelemetryLoggerConstants.BotMsgDeleteEvent, properties);
                return;
            }
        }

        public class OverrideFillLogger : TelemetryLoggerMiddleware
        {
            public OverrideFillLogger(IBotTelemetryClient telemetryClient, bool logPersonalInformation = false)
                : base(telemetryClient, logPersonalInformation)
            {
            }

            protected override async Task OnReceiveActivityAsync(Activity activity, CancellationToken cancellation)
            {
                var properties = new Dictionary<string, string>
                {
                    { "foo", "bar" },
                    { "ImportantProperty", "ImportantValue" },
                };
                TelemetryClient.TrackEvent(TelemetryLoggerConstants.BotMsgReceiveEvent, await this.FillReceiveEventPropertiesAsync(activity, properties));
                return;
            }

            protected override async Task OnSendActivityAsync(Activity activity, CancellationToken cancellation)
            {
                var properties = new Dictionary<string, string>
                {
                    { "foo", "bar" },
                    { "ImportantProperty", "ImportantValue" },
                };
                TelemetryClient.TrackEvent(TelemetryLoggerConstants.BotMsgSendEvent, await this.FillSendEventPropertiesAsync(activity, properties));
                return;
            }

            protected override async Task OnUpdateActivityAsync(Activity activity, CancellationToken cancellation)
            {
                var properties = new Dictionary<string, string>
                {
                    { "foo", "bar" },
                    { "ImportantProperty", "ImportantValue" },
                };
                TelemetryClient.TrackEvent(TelemetryLoggerConstants.BotMsgUpdateEvent, await this.FillUpdateEventPropertiesAsync(activity, properties));
                return;
            }

            protected override async Task OnDeleteActivityAsync(Activity activity, CancellationToken cancellation)
            {
                var properties = new Dictionary<string, string>
                {
                    { "foo", "bar" },
                    { "ImportantProperty", "ImportantValue" },
                };
                TelemetryClient.TrackEvent(TelemetryLoggerConstants.BotMsgDeleteEvent, await FillDeleteEventPropertiesAsync(activity, properties));
                return;
            }
        }
    }
}
