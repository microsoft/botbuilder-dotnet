// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Integration.ApplicationInsights.Core.Tests
{
    [TestClass]
    [TestCategory("ApplicationInsights")]
    public class TelemetryInitializerTests
    {
        [TestMethod]
        public void VerifyAllTelemtryPropoerties()
        {
            var configuration = new TelemetryConfiguration();
            var sentItems = new List<ITelemetry>();
            var mockTelemetryChannel = new Mock<ITelemetryChannel>();
            mockTelemetryChannel.Setup(c => c.Send(It.IsAny<ITelemetry>()))
                            .Callback<ITelemetry>((telemetry) => sentItems.Add(telemetry))
                            .Verifiable();
            configuration.TelemetryChannel = mockTelemetryChannel.Object;
            configuration.InstrumentationKey = Guid.NewGuid().ToString();

            // Mock http context
            var httpContext = new Mock<HttpContext>();
            IDictionary<object, object> items = new Dictionary<object, object>();
            httpContext.SetupProperty(c => c.Items, items);
            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            httpContextAccessor.SetupProperty(c => c.HttpContext, httpContext.Object);

            // Simulate what Middleware does to read body
            var fromID = "FROMID";
            var channelID = "CHANNELID";
            var conversationID = "CONVERSATIONID";
            var sessionId = StringUtils.Hash(conversationID);
            var activityID = "ACTIVITYID";
            var activity = Activity.CreateMessageActivity();
            activity.From = new ChannelAccount(fromID);
            activity.ChannelId = channelID;
            activity.Conversation = new ConversationAccount(false, "CONVOTYPE", conversationID);
            activity.Id = activityID;
            var activityBody = JObject.FromObject(activity);
            items.Add(TelemetryBotIdInitializer.BotActivityKey, activityBody);
            configuration.TelemetryInitializers.Add(new TelemetryBotIdInitializer(httpContextAccessor.Object));
            var telemetryClient = new TelemetryClient(configuration);

            telemetryClient.TrackEvent("test", new Dictionary<string, string>() { { "hello", "value" } }, new Dictionary<string, double>() { { "metric", 0.6 } });

            Assert.IsTrue(sentItems.Count == 1);
            var telem = sentItems[0] as EventTelemetry;
            Assert.IsTrue(telem != null);
            Assert.IsTrue(telem.Properties["activityId"] == activityID);
            Assert.IsTrue(telem.Properties["activityType"] == "message");
            Assert.IsTrue(telem.Properties["channelId"] == "CHANNELID");
            Assert.IsTrue(telem.Properties["conversationId"] == conversationID);
            Assert.IsTrue(telem.Context.Session.Id == sessionId);
            Assert.IsTrue(telem.Context.User.Id == channelID + fromID);
            Assert.IsTrue(telem.Properties["hello"] == "value");
            Assert.IsTrue(telem.Metrics["metric"] == 0.6);
        }

        [TestMethod]
        public void VerifyOverriddenProperties()
        {
            var configuration = new TelemetryConfiguration();
            var sentItems = new List<ITelemetry>();
            var mockTelemetryChannel = new Mock<ITelemetryChannel>();
            mockTelemetryChannel.Setup(c => c.Send(It.IsAny<ITelemetry>()))
                            .Callback<ITelemetry>((telemetry) => sentItems.Add(telemetry))
                            .Verifiable();
            configuration.TelemetryChannel = mockTelemetryChannel.Object;
            configuration.InstrumentationKey = Guid.NewGuid().ToString();

            // Mock http context
            var httpContext = new Mock<HttpContext>();
            IDictionary<object, object> items = new Dictionary<object, object>();
            httpContext.SetupProperty(c => c.Items, items);
            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            httpContextAccessor.SetupProperty(c => c.HttpContext, httpContext.Object);

            // Simulate what Middleware does to read body
            var fromID = "FROMID";
            var channelID = "CHANNELID";
            var conversationID = "CONVERSATIONID";
            var activityID = "ACTIVITYID";
            var sessionId = StringUtils.Hash(conversationID);
            var activity = Activity.CreateMessageActivity();
            activity.From = new ChannelAccount(fromID);
            activity.ChannelId = channelID;
            activity.Conversation = new ConversationAccount(false, "CONVOTYPE", conversationID);
            activity.Id = activityID;
            var activityBody = JObject.FromObject(activity);
            items.Add(TelemetryBotIdInitializer.BotActivityKey, activityBody);
            configuration.TelemetryInitializers.Add(new TelemetryBotIdInitializer(httpContextAccessor.Object));
            var telemetryClient = new TelemetryClient(configuration);
            var activityIdValue = "Oh yeah I did it";
            var channelIdValue = "Hello";
            var activityTypeValue = "Breaking all the rules";

            // Should not throw.  This implicitly calls the initializer.
            // Note: We are setting properties that should be populated by the TelemetryInitailizer.
            // We honor overrides.
            var metrics = new Dictionary<string, double>()
            {
                {
                    "metric",
                    0.6
                },
            };
            telemetryClient.TrackEvent("test", new Dictionary<string, string>()
            {
                { "activityId", activityIdValue },  // The activityId can be overridden.
                { "channelId", channelIdValue },
                { "activityType", activityTypeValue },
            },
#pragma warning disable SA1117 // Parameters should be on same line or separate lines
            metrics);
#pragma warning restore SA1117 // Parameters should be on same line or separate lines

            Assert.IsTrue(sentItems.Count == 1);
            var telem = sentItems[0] as EventTelemetry;
            Assert.IsTrue(telem != null);

            Assert.IsTrue(telem.Context.Session.Id == sessionId);
            Assert.IsTrue(telem.Context.User.Id == channelID + fromID);

            // The TelemetryInitializer honors being overridden
            // What we get out should be what we originally put in, and not what the Initializer
            // normally does.
            Assert.IsFalse(telem.Properties["activityId"] == activityID);
            Assert.IsTrue(telem.Properties["activityId"] == activityIdValue);
            Assert.IsTrue(telem.Properties["channelId"] == channelIdValue);
            Assert.IsFalse(telem.Properties["channelId"] == "CHANNELID");
            Assert.IsTrue(telem.Properties["activityType"] == activityTypeValue);
            Assert.IsFalse(telem.Properties["activityType"] == "message");
            Assert.IsTrue(telem.Properties["conversationId"] == conversationID);
            Assert.IsTrue(telem.Metrics["metric"] == 0.6);
        }

        [TestMethod]
        public void VerifyTraceProperties()
        {
            var configuration = new TelemetryConfiguration();
            var sentItems = new List<ITelemetry>();
            var mockTelemetryChannel = new Mock<ITelemetryChannel>();
            mockTelemetryChannel.Setup(c => c.Send(It.IsAny<ITelemetry>()))
                            .Callback<ITelemetry>((telemetry) => sentItems.Add(telemetry))
                            .Verifiable();
            configuration.TelemetryChannel = mockTelemetryChannel.Object;
            configuration.InstrumentationKey = Guid.NewGuid().ToString();

            // Mock http context
            var httpContext = new Mock<HttpContext>();
            IDictionary<object, object> items = new Dictionary<object, object>();
            httpContext.SetupProperty(c => c.Items, items);
            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            httpContextAccessor.SetupProperty(c => c.HttpContext, httpContext.Object);

            // Simulate what Middleware does to read body
            var fromID = "FROMID";
            var channelID = "CHANNELID";
            var conversationID = "CONVERSATIONID";
            var sessionId = StringUtils.Hash(conversationID);
            var activityID = "ACTIVITYID";
            var activity = Activity.CreateMessageActivity();
            activity.From = new ChannelAccount(fromID);
            activity.ChannelId = channelID;
            activity.Conversation = new ConversationAccount(false, "CONVOTYPE", conversationID);
            activity.Id = activityID;
            var activityBody = JObject.FromObject(activity);
            items.Add(TelemetryBotIdInitializer.BotActivityKey, activityBody);
            configuration.TelemetryInitializers.Add(new TelemetryBotIdInitializer(httpContextAccessor.Object));
            var telemetryClient = new TelemetryClient(configuration);

            telemetryClient.TrackTrace("test");

            Assert.IsTrue(sentItems.Count == 1);
            var telem = sentItems[0] as TraceTelemetry;
            Assert.IsTrue(telem != null);
            Assert.IsTrue(telem.Properties["activityId"] == activityID);
            Assert.IsTrue(telem.Properties["activityType"] == "message");
            Assert.IsTrue(telem.Properties["channelId"] == "CHANNELID");
            Assert.IsTrue(telem.Properties["conversationId"] == conversationID);
            Assert.IsTrue(telem.Context.Session.Id == sessionId);
            Assert.IsTrue(telem.Context.User.Id == channelID + fromID);
        }

        [TestMethod]
        public void VerifyRequestProperties()
        {
            var configuration = new TelemetryConfiguration();
            var sentItems = new List<ITelemetry>();
            var mockTelemetryChannel = new Mock<ITelemetryChannel>();
            mockTelemetryChannel.Setup(c => c.Send(It.IsAny<ITelemetry>()))
                            .Callback<ITelemetry>((telemetry) => sentItems.Add(telemetry))
                            .Verifiable();
            configuration.TelemetryChannel = mockTelemetryChannel.Object;
            configuration.InstrumentationKey = Guid.NewGuid().ToString();

            // Mock http context
            var httpContext = new Mock<HttpContext>();
            IDictionary<object, object> items = new Dictionary<object, object>();
            httpContext.SetupProperty(c => c.Items, items);
            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            httpContextAccessor.SetupProperty(c => c.HttpContext, httpContext.Object);

            // Simulate what Middleware does to read body
            var fromID = "FROMID";
            var channelID = "CHANNELID";
            var conversationID = "CONVERSATIONID";
            var sessionId = StringUtils.Hash(conversationID);
            var activityID = "ACTIVITYID";
            var activity = Activity.CreateMessageActivity();
            activity.From = new ChannelAccount(fromID);
            activity.ChannelId = channelID;
            activity.Conversation = new ConversationAccount(false, "CONVOTYPE", conversationID);
            activity.Id = activityID;
            var activityBody = JObject.FromObject(activity);
            items.Add(TelemetryBotIdInitializer.BotActivityKey, activityBody);
            configuration.TelemetryInitializers.Add(new TelemetryBotIdInitializer(httpContextAccessor.Object));
            var telemetryClient = new TelemetryClient(configuration);

            telemetryClient.TrackRequest("Foo", DateTimeOffset.Now, TimeSpan.FromSeconds(1.0), "response", true);

            Assert.IsTrue(sentItems.Count == 1);
            var telem = sentItems[0] as RequestTelemetry;
            Assert.IsTrue(telem != null);
            Assert.IsTrue(telem.Properties["activityId"] == activityID);
            Assert.IsTrue(telem.Properties["activityType"] == "message");
            Assert.IsTrue(telem.Properties["channelId"] == "CHANNELID");
            Assert.IsTrue(telem.Properties["conversationId"] == conversationID);
            Assert.IsTrue(telem.Context.Session.Id == sessionId);
            Assert.IsTrue(telem.Context.User.Id == channelID + fromID);
        }

        [TestMethod]
        public void VerifyDependencyProperties()
        {
            var configuration = new TelemetryConfiguration();
            var sentItems = new List<ITelemetry>();
            var mockTelemetryChannel = new Mock<ITelemetryChannel>();
            mockTelemetryChannel.Setup(c => c.Send(It.IsAny<ITelemetry>()))
                            .Callback<ITelemetry>((telemetry) => sentItems.Add(telemetry))
                            .Verifiable();
            configuration.TelemetryChannel = mockTelemetryChannel.Object;
            configuration.InstrumentationKey = Guid.NewGuid().ToString();

            // Mock http context
            var httpContext = new Mock<HttpContext>();
            IDictionary<object, object> items = new Dictionary<object, object>();
            httpContext.SetupProperty(c => c.Items, items);
            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            httpContextAccessor.SetupProperty(c => c.HttpContext, httpContext.Object);

            // Simulate what Middleware does to read body
            var fromID = "FROMID";
            var channelID = "CHANNELID";
            var conversationID = "CONVERSATIONID";
            var sessionId = StringUtils.Hash(conversationID);
            var activityID = "ACTIVITYID";
            var activity = Activity.CreateMessageActivity();
            activity.From = new ChannelAccount(fromID);
            activity.ChannelId = channelID;
            activity.Conversation = new ConversationAccount(false, "CONVOTYPE", conversationID);
            activity.Id = activityID;
            var activityBody = JObject.FromObject(activity);
            items.Add(TelemetryBotIdInitializer.BotActivityKey, activityBody);
            configuration.TelemetryInitializers.Add(new TelemetryBotIdInitializer(httpContextAccessor.Object));
            var telemetryClient = new TelemetryClient(configuration);

            telemetryClient.TrackDependency("Foo", "Bar", "Data", DateTimeOffset.Now, TimeSpan.FromSeconds(1.0), true);

            Assert.IsTrue(sentItems.Count == 1);
            var telem = sentItems[0] as DependencyTelemetry;
            Assert.IsTrue(telem != null);
            Assert.IsTrue(telem.Properties["activityId"] == activityID);
            Assert.IsTrue(telem.Properties["activityType"] == "message");
            Assert.IsTrue(telem.Properties["channelId"] == "CHANNELID");
            Assert.IsTrue(telem.Properties["conversationId"] == conversationID);
            Assert.IsTrue(telem.Context.Session.Id == sessionId);
            Assert.IsTrue(telem.Context.User.Id == channelID + fromID);
        }

        [TestMethod]
        public void InvalidMessage_NoConversation()
        {
            var configuration = new TelemetryConfiguration();
            var sentItems = new List<ITelemetry>();
            var mockTelemetryChannel = new Mock<ITelemetryChannel>();
            mockTelemetryChannel.Setup(c => c.Send(It.IsAny<ITelemetry>()))
                            .Callback<ITelemetry>((telemetry) => sentItems.Add(telemetry))
                            .Verifiable();
            configuration.TelemetryChannel = mockTelemetryChannel.Object;
            configuration.InstrumentationKey = Guid.NewGuid().ToString();

            // Mock http context
            var httpContext = new Mock<HttpContext>();
            IDictionary<object, object> items = new Dictionary<object, object>();
            httpContext.SetupProperty(c => c.Items, items);
            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            httpContextAccessor.SetupProperty(c => c.HttpContext, httpContext.Object);

            // Simulate what Middleware does to read body
            var fromID = "FROMID";
            var channelID = "CHANNELID";
            var activityID = "ACTIVITYID";
            var activity = Activity.CreateMessageActivity();
            activity.From = new ChannelAccount(fromID);
            activity.ChannelId = channelID;
            activity.Id = activityID;
            var activityBody = JObject.FromObject(activity);
            items.Add(TelemetryBotIdInitializer.BotActivityKey, activityBody);
            configuration.TelemetryInitializers.Add(new TelemetryBotIdInitializer(httpContextAccessor.Object));
            var telemetryClient = new TelemetryClient(configuration);

            telemetryClient.TrackEvent("test", new Dictionary<string, string>() { { "hello", "value" } }, new Dictionary<string, double>() { { "metric", 0.6 } });

            Assert.IsTrue(sentItems.Count == 1);
            var telem = sentItems[0] as EventTelemetry;
            var properties = (ISupportProperties)telem;
            Assert.IsTrue(telem != null);
            Assert.IsTrue(properties.Properties["activityId"] == activityID);
            Assert.IsTrue(properties.Properties["activityType"] == "message");
            Assert.IsTrue(telem.Properties["channelId"] == "CHANNELID");
            Assert.IsTrue(telem.Context.User.Id == channelID + fromID);
            Assert.IsTrue(telem.Properties["hello"] == "value");
            Assert.IsTrue(telem.Metrics["metric"] == 0.6);
        }

        [TestMethod]
        [TestCategory("Telemetry")]
        public async Task Telemetry_InitializerMiddleware_LogActivities_Enabled()
        {
            // Arrange
            var mockTelemetryClient = new Mock<IBotTelemetryClient>();
            var mockHttpContextAccessor = new Mock<HttpContextAccessor>();
            mockHttpContextAccessor.Object.HttpContext = new DefaultHttpContext();

            var adapter = new TestAdapter()
                .Use(new TelemetryInitializerMiddleware(
                    mockHttpContextAccessor.Object,
                    new TelemetryLoggerMiddleware(mockTelemetryClient.Object, false),
                    logActivityTelemetry: true));
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
                    .AssertReply((activity) => Assert.AreEqual(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:foo")
                .Send("bar")
                    .AssertReply((activity) => Assert.AreEqual(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:bar")
                .StartTestAsync();

            // Assert
            Assert.IsNotNull(mockHttpContextAccessor.Object.HttpContext.Items);
            Assert.IsTrue(mockHttpContextAccessor.Object.HttpContext.Items.Count == 1);
            Assert.AreEqual(mockTelemetryClient.Invocations.Count, 6);
        }

        [TestMethod]
        [TestCategory("Telemetry")]
        public async Task Telemetry_InitializerMiddleware_LogActivities_Disabled()
        {
            // Arrange
            var mockTelemetryClient = new Mock<IBotTelemetryClient>();
            var mockHttpContextAccessor = new Mock<HttpContextAccessor>();
            mockHttpContextAccessor.Object.HttpContext = new DefaultHttpContext();

            var adapter = new TestAdapter()
                .Use(new TelemetryInitializerMiddleware(
                    mockHttpContextAccessor.Object,
                    new TelemetryLoggerMiddleware(mockTelemetryClient.Object, false),
                    logActivityTelemetry: false));
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
                    .AssertReply((activity) => Assert.AreEqual(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:foo")
                .Send("bar")
                    .AssertReply((activity) => Assert.AreEqual(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:bar")
                .StartTestAsync();

            // Assert
            Assert.IsNotNull(mockHttpContextAccessor.Object.HttpContext.Items);
            Assert.IsTrue(mockHttpContextAccessor.Object.HttpContext.Items.Count == 1);
            Assert.AreEqual(mockTelemetryClient.Invocations.Count, 0);
        }

        [TestMethod]
        [TestCategory("Telemetry")]
        public async Task Telemetry_InitializerMiddleware_Null_HttpContext_NoError()
        {
            // Arrange
            var mockTelemetryClient = new Mock<IBotTelemetryClient>();
            var mockHttpContextAccessor = new Mock<HttpContextAccessor>();

            var adapter = new TestAdapter()
                .Use(new TelemetryInitializerMiddleware(
                    mockHttpContextAccessor.Object,
                    new TelemetryLoggerMiddleware(mockTelemetryClient.Object, false),
                    logActivityTelemetry: false));
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
                    .AssertReply((activity) => Assert.AreEqual(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:foo")
                .Send("bar")
                    .AssertReply((activity) => Assert.AreEqual(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:bar")
                .StartTestAsync();

            // Assert
            Assert.IsNull(mockHttpContextAccessor.Object.HttpContext);
            Assert.AreEqual(mockTelemetryClient.Invocations.Count, 0);
        }
    }
}
