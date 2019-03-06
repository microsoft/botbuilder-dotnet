// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
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
        public async Task Telemetry_LogActivities()
        {
            var mockTelemetryClient = new Mock<IBotTelemetryClient>();
            TestAdapter adapter = new TestAdapter()
                .Use(new TelemetryLoggerMiddleware(mockTelemetryClient.Object, logPersonalInformation: true));
            string conversationId = null;

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
        public async Task Transcript_LogUpdateActivities()
        {
            var mockTelemetryClient = new Mock<IBotTelemetryClient>();
            TestAdapter adapter = new TestAdapter()
                .Use(new TelemetryLoggerMiddleware(mockTelemetryClient.Object, logPersonalInformation: true));
            string conversationId = null;
            Activity activityToUpdate = null;
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
            var mockTelemetryClient = new Mock<IBotTelemetryClient>();
            TestAdapter adapter = new TestAdapter()
                .Use(new TelemetryLoggerMiddleware(mockTelemetryClient.Object, logPersonalInformation: true));
            string conversationId = null;
            string activityId = null;
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
            Assert.AreEqual(mockTelemetryClient.Invocations.Count, 4);
            Assert.AreEqual(mockTelemetryClient.Invocations[3].Arguments[0], "BotMessageDelete"); // Check delete message
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).Count == 3);
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("recipientId"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("conversationId"));
            Assert.IsTrue(((Dictionary<string, string>)mockTelemetryClient.Invocations[3].Arguments[1]).ContainsKey("conversationName"));
        }
    }
}
