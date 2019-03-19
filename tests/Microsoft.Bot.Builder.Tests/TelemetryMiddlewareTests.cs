// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    public class TelemetryMiddlewareTests
    {
        [TestMethod]
        [TestCategory("Telemetry")]
        public async Task Telemetry_NullTelemetryClient()
        {
            // Arrange / Act
            //
            // Note: The TelemetryClient will most likely be DI'd in, and handling a null
            // TelemetryClient should be handled by placing the NullBotTelemetryClient().
            //
            var logger = new TelemetryLoggerMiddleware(null, logPersonalInformation: true);

            // Assert
            Assert.IsNotNull(logger);
        }

        [TestMethod]
        [TestCategory("Telemetry")]
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
                    RelatesTo = context.Activity.RelatesTo
                };
                await context.SendActivityAsync(typingActivity);
                await Task.Delay(500);
                await context.SendActivityAsync("echo:" + context.Activity.Text);
            })
                .Send("foo")
                    .AssertReply((activity) => Assert.AreEqual(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:foo")
                .Send("bar")
                    .AssertReply((activity) => Assert.AreEqual(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:bar")
                .StartTestAsync();

            // Assert
            Assert.AreEqual(mockTelemetryClient.Invocations.Count, 6);
            Assert.AreEqual(mockTelemetryClient.Invocations[0].Arguments[0], "BotMessageReceived"); // Check initial message
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).Count == 7);
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("fromId"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("conversationName"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("locale"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("recipientId"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("recipientName"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("fromName"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("text"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1])["text"] == "foo");

            Assert.AreEqual(mockTelemetryClient.Invocations[1].Arguments[0], "BotMessageSend"); // Check Typing message
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).Count == 5);
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("replyActivityId"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("recipientId"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("conversationName"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("locale"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("recipientName"));

            Assert.AreEqual(mockTelemetryClient.Invocations[2].Arguments[0], "BotMessageSend"); // Check message reply
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).Count == 6);
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("replyActivityId"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("recipientId"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("conversationName"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("locale"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("recipientName"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("text"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1])["text"] == "echo:foo");

            Assert.AreEqual(mockTelemetryClient.Invocations[3].Arguments[0], "BotMessageReceived"); // Check bar message
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).Count == 7);
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("fromId"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("conversationName"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("locale"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("recipientId"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("recipientName"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("fromName"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("text"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1])["text"] == "bar");
        }

        [TestMethod]
        [TestCategory("Telemetry")]
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
                    RelatesTo = context.Activity.RelatesTo
                };
                await context.SendActivityAsync(typingActivity);
                await Task.Delay(500);
                await context.SendActivityAsync("echo:" + context.Activity.Text);
            })
                .Send("foo")
                    .AssertReply((activity) => Assert.AreEqual(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:foo")
                .Send("bar")
                    .AssertReply((activity) => Assert.AreEqual(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:bar")
                .StartTestAsync();

            // Assert
            Assert.AreEqual(mockTelemetryClient.Invocations.Count, 6);
            Assert.AreEqual(mockTelemetryClient.Invocations[0].Arguments[0], "BotMessageReceived"); // Check initial message
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).Count == 5);
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("fromId"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("conversationName"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("locale"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("recipientId"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("recipientName"));
            Assert.IsFalse(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("fromName"));
            Assert.IsFalse(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("text"));

            Assert.AreEqual(mockTelemetryClient.Invocations[1].Arguments[0], "BotMessageSend"); // Check Typing message
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).Count == 4);
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("replyActivityId"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("recipientId"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("conversationName"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("locale"));
            Assert.IsFalse(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("recipientName"));

            Assert.AreEqual(mockTelemetryClient.Invocations[2].Arguments[0], "BotMessageSend"); // Check message reply
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).Count == 4);
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("replyActivityId"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("recipientId"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("conversationName"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("locale"));
            Assert.IsFalse(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("recipientName"));
            Assert.IsFalse(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("text"));

            Assert.AreEqual(mockTelemetryClient.Invocations[3].Arguments[0], "BotMessageReceived"); // Check bar message
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).Count == 5);
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("fromId"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("conversationName"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("locale"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("recipientId"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("recipientName"));
            Assert.IsFalse(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("fromName"));
            Assert.IsFalse(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("text"));
        }


        [TestMethod]
        [TestCategory("Telemetry")]
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
            Assert.AreEqual(mockTelemetryClient.Invocations.Count, 4);
            Assert.AreEqual(mockTelemetryClient.Invocations[3].Arguments[0], "BotMessageUpdate"); // Check update message
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).Count == 5);
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("recipientId"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("conversationId"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("conversationName"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("locale"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("text"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1])["text"] == "new response");
        }


        [TestMethod]
        [TestCategory("Middleware")]
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
            Assert.AreEqual(mockTelemetryClient.Invocations.Count, 4);
            Assert.AreEqual(mockTelemetryClient.Invocations[3].Arguments[0], "BotMessageDelete"); // Check delete message
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).Count == 3);
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("recipientId"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("conversationId"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("conversationName"));
        }

        [TestMethod]
        [TestCategory("Telemetry")]
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
                    RelatesTo = context.Activity.RelatesTo
                };
                await context.SendActivityAsync(typingActivity);
                await Task.Delay(500);
                await context.SendActivityAsync("echo:" + context.Activity.Text);
            })
                .Send("foo")
                    .AssertReply((activity) => Assert.AreEqual(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:foo")
                .Send("bar")
                    .AssertReply((activity) => Assert.AreEqual(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:bar")
                .StartTestAsync();

            // Assert
            Assert.AreEqual(mockTelemetryClient.Invocations.Count, 8);
            Assert.AreEqual(mockTelemetryClient.Invocations[0].Arguments[0], TelemetryLoggerConstants.BotMsgReceiveEvent); // Check initial message
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).Count == 2);
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("foo"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1])["foo"] == "bar");
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("ImportantProperty"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1])["ImportantProperty"] == "ImportantValue");

            Assert.AreEqual(mockTelemetryClient.Invocations[1].Arguments[0], "MyReceive" ); // Check my message
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("fromId"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("conversationName"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("locale"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("recipientId"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("recipientName"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("fromName"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("text"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1])["text"] == "foo");

            Assert.AreEqual(mockTelemetryClient.Invocations[2].Arguments[0], "BotMessageSend"); // Check Typing message
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).Count == 5);
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("replyActivityId"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("recipientId"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("conversationName"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("locale"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("recipientName"));

            Assert.AreEqual(mockTelemetryClient.Invocations[3].Arguments[0], "BotMessageSend"); // Check message reply
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).Count == 6);
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("replyActivityId"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("recipientId"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("conversationName"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("locale"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("recipientName"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("text"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1])["text"] == "echo:foo");
        }

        [TestMethod]
        [TestCategory("Telemetry")]
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
                    RelatesTo = context.Activity.RelatesTo
                };
                await context.SendActivityAsync(typingActivity);
                await Task.Delay(500);
                await context.SendActivityAsync("echo:" + context.Activity.Text);
            })
                .Send("foo")
                    .AssertReply((activity) => Assert.AreEqual(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:foo")
                .Send("bar")
                    .AssertReply((activity) => Assert.AreEqual(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:bar")
                .StartTestAsync();

            Assert.AreEqual(mockTelemetryClient.Invocations.Count, 10);
            Assert.AreEqual(mockTelemetryClient.Invocations[0].Arguments[0], TelemetryLoggerConstants.BotMsgReceiveEvent); // Check initial message
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).Count == 7);
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("fromId"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("conversationName"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("locale"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("recipientId"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("recipientName"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("fromName"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("text"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1])["text"] == "foo");

            Assert.AreEqual(mockTelemetryClient.Invocations[1].Arguments[0], TelemetryLoggerConstants.BotMsgSendEvent); // Check Typing message (1 of 2)
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).Count == 2);
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("foo"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1])["foo"] == "bar");
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("ImportantProperty"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1])["ImportantProperty"] == "ImportantValue");

            Assert.AreEqual(mockTelemetryClient.Invocations[2].Arguments[0], "MySend"); // Check custom message
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).Count == 5);
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("replyActivityId"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("recipientId"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("conversationName"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("locale"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[2].Arguments[1]).ContainsKey("recipientName"));
        }

        [TestMethod]
        [TestCategory("Telemetry")]
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

            Assert.AreEqual(mockTelemetryClient.Invocations.Count, 5);
            Assert.AreEqual(mockTelemetryClient.Invocations[3].Arguments[0], TelemetryLoggerConstants.BotMsgUpdateEvent); // Check update message
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).Count == 2);
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("foo"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1])["foo"] == "bar");
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("ImportantProperty"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1])["ImportantProperty"] == "ImportantValue");

            Assert.AreEqual(mockTelemetryClient.Invocations[4].Arguments[0], TelemetryLoggerConstants.BotMsgDeleteEvent); // Check delete message
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[4].Arguments[1]).Count == 2);
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[4].Arguments[1]).ContainsKey("foo"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[4].Arguments[1])["foo"] == "bar");
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[4].Arguments[1]).ContainsKey("ImportantProperty"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[4].Arguments[1])["ImportantProperty"] == "ImportantValue");
        }

        [TestMethod]
        [TestCategory("Telemetry")]
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



            Assert.AreEqual(mockTelemetryClient.Invocations[0].Arguments[0], TelemetryLoggerConstants.BotMsgReceiveEvent); // Check Receive message
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("fromId"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("conversationName"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("locale"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("recipientId"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("recipientName"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("fromName"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1]).ContainsKey("text"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[0].Arguments[1])["text"] == "foo");

            Assert.AreEqual(mockTelemetryClient.Invocations[1].Arguments[0], TelemetryLoggerConstants.BotMsgSendEvent); // Check Send message
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).Count == 8);
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("replyActivityId"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("recipientId"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("conversationName"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("locale"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("recipientName"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("foo"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1]).ContainsKey("text"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1])["text"] == "response");
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1])["foo"] == "bar");
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[1].Arguments[1])["ImportantProperty"] == "ImportantValue");

            Assert.AreEqual(mockTelemetryClient.Invocations[3].Arguments[0], TelemetryLoggerConstants.BotMsgUpdateEvent); // Check Update message
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).Count == 7);
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("conversationId"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("conversationName"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("locale"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("foo"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("text"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1])["text"] == "new response");
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1])["foo"] == "bar");
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1])["ImportantProperty"] == "ImportantValue");

            Assert.AreEqual(mockTelemetryClient.Invocations[4].Arguments[0], TelemetryLoggerConstants.BotMsgDeleteEvent); // Check Delete message
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[4].Arguments[1]).Count == 5);
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[4].Arguments[1]).ContainsKey("recipientId"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[4].Arguments[1]).ContainsKey("conversationName"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[4].Arguments[1]).ContainsKey("conversationId"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[4].Arguments[1]).ContainsKey("foo"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[4].Arguments[1])["foo"] == "bar");
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[4].Arguments[1])["ImportantProperty"] == "ImportantValue");
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
                    { "foo" , "bar" },
                    { "ImportantProperty" , "ImportantValue" },
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
                    { "foo" , "bar" },
                    { "ImportantProperty" , "ImportantValue" },
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


            protected override async Task OnUpdateActivityAsync(Activity activity, CancellationToken cancellation)
            {
                var properties = new Dictionary<string, string>
                {
                    { "foo" , "bar" },
                    { "ImportantProperty" , "ImportantValue" },
                };
                TelemetryClient.TrackEvent(TelemetryLoggerConstants.BotMsgUpdateEvent, properties);
                return;
            }
            protected override async Task OnDeleteActivityAsync(Activity activity, CancellationToken cancellation)
            {
                var properties = new Dictionary<string, string>
                {
                    { "foo" , "bar" },
                    { "ImportantProperty" , "ImportantValue" },
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
                    { "foo" , "bar" },
                    { "ImportantProperty" , "ImportantValue" },
                };
                TelemetryClient.TrackEvent(TelemetryLoggerConstants.BotMsgReceiveEvent, await this.FillReceiveEventPropertiesAsync(activity, properties));
                return;
            }


            protected override async Task OnSendActivityAsync(Activity activity, CancellationToken cancellation)
            {
                var properties = new Dictionary<string, string>
                {
                    { "foo" , "bar" },
                    { "ImportantProperty" , "ImportantValue" },
                };
                TelemetryClient.TrackEvent(TelemetryLoggerConstants.BotMsgSendEvent, await this.FillSendEventPropertiesAsync(activity, properties));
                return;
            }


            protected override async Task OnUpdateActivityAsync(Activity activity, CancellationToken cancellation)
            {
                var properties = new Dictionary<string, string>
                {
                    { "foo" , "bar" },
                    { "ImportantProperty" , "ImportantValue" },
                };
                TelemetryClient.TrackEvent(TelemetryLoggerConstants.BotMsgUpdateEvent, await this.FillUpdateEventPropertiesAsync(activity, properties));
                return;
            }
            protected override async Task OnDeleteActivityAsync(Activity activity, CancellationToken cancellation)
            {
                var properties = new Dictionary<string, string>
                {
                    { "foo" , "bar" },
                    { "ImportantProperty" , "ImportantValue" },
                };
                TelemetryClient.TrackEvent(TelemetryLoggerConstants.BotMsgDeleteEvent, await FillDeleteEventPropertiesAsync(activity, properties));
                return;
            }
        }


    }
}

