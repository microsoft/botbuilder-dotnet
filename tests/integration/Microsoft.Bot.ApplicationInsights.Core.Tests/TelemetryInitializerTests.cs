// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Integration.ApplicationInsights.Core.Tests
{
    [Trait("TestCategory", "ApplicationInsights")]
    public class TelemetryInitializerTests
    {
        [Fact]
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

            Assert.Single(sentItems);
            var telem = sentItems[0] as EventTelemetry;
            Assert.NotNull(telem);
            Assert.Equal(activityID, telem.Properties["activityId"]);
            Assert.Equal("message", telem.Properties["activityType"]);
            Assert.Equal("CHANNELID", telem.Properties["channelId"]);
            Assert.Equal(conversationID, telem.Properties["conversationId"]);
            Assert.Equal(sessionId, telem.Context.Session.Id);
            Assert.Equal(channelID + fromID, telem.Context.User.Id);
            Assert.Equal("value", telem.Properties["hello"]);
            Assert.Equal(0.6, telem.Metrics["metric"]);
        }

        [Fact]
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

            var properties = new Dictionary<string, string>()
            {
                { "activityId", activityIdValue },  // The activityId can be overridden.
                { "channelId", channelIdValue },
                { "activityType", activityTypeValue },
            };

            telemetryClient.TrackEvent("test", properties, metrics);

            Assert.Single(sentItems);
            var telem = sentItems[0] as EventTelemetry;
            Assert.NotNull(telem);

            Assert.Equal(sessionId, telem.Context.Session.Id);
            Assert.Equal(channelID + fromID, telem.Context.User.Id);

            // The TelemetryInitializer honors being overridden
            // What we get out should be what we originally put in, and not what the Initializer
            // normally does.
            Assert.NotEqual(activityID, telem.Properties["activityId"]);
            Assert.Equal(activityIdValue, telem.Properties["activityId"]);
            Assert.Equal(channelIdValue, telem.Properties["channelId"]);
            Assert.NotEqual("CHANNELID", telem.Properties["channelId"]);
            Assert.Equal(activityTypeValue, telem.Properties["activityType"]);
            Assert.NotEqual("message", telem.Properties["activityType"]);
            Assert.Equal(conversationID, telem.Properties["conversationId"]);
            Assert.Equal(0.6, telem.Metrics["metric"]);
        }

        [Fact]
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

            Assert.Single(sentItems);
            var telem = sentItems[0] as TraceTelemetry;
            Assert.NotNull(telem);
            Assert.Equal(activityID, telem.Properties["activityId"]);
            Assert.Equal("message", telem.Properties["activityType"]);
            Assert.Equal("CHANNELID", telem.Properties["channelId"]);
            Assert.Equal(conversationID, telem.Properties["conversationId"]);
            Assert.Equal(sessionId, telem.Context.Session.Id);
            Assert.Equal(channelID + fromID, telem.Context.User.Id);
        }

        [Fact]
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

            Assert.Single(sentItems);
            var telem = sentItems[0] as RequestTelemetry;
            Assert.NotNull(telem);
            Assert.Equal(activityID, telem.Properties["activityId"]);
            Assert.Equal("message", telem.Properties["activityType"]);
            Assert.Equal("CHANNELID", telem.Properties["channelId"]);
            Assert.Equal(conversationID, telem.Properties["conversationId"]);
            Assert.Equal(sessionId, telem.Context.Session.Id);
            Assert.Equal(channelID + fromID, telem.Context.User.Id);
        }

        [Fact]
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

            Assert.Single(sentItems);
            var telem = sentItems[0] as DependencyTelemetry;
            Assert.NotNull(telem);
            Assert.Equal(activityID, telem.Properties["activityId"]);
            Assert.Equal("message", telem.Properties["activityType"]);
            Assert.Equal("CHANNELID", telem.Properties["channelId"]);
            Assert.Equal(conversationID, telem.Properties["conversationId"]);
            Assert.Equal(sessionId, telem.Context.Session.Id);
            Assert.Equal(channelID + fromID, telem.Context.User.Id);
        }

        [Fact]
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

            Assert.Single(sentItems);
            var telem = sentItems[0] as EventTelemetry;
            var properties = (ISupportProperties)telem;
            Assert.NotNull(telem);
            Assert.Equal(activityID, properties.Properties["activityId"]);
            Assert.Equal("message", properties.Properties["activityType"]);
            Assert.Equal("CHANNELID", telem.Properties["channelId"]);
            Assert.Equal(channelID + fromID, telem.Context.User.Id);
            Assert.Equal("value", telem.Properties["hello"]);
            Assert.Equal(0.6, telem.Metrics["metric"]);
        }

        [Fact]
        [Trait("TestCategory", "Telemetry")]
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
                    .AssertReply((activity) => Assert.Equal(ActivityTypes.Typing, activity.Type))
                    .AssertReply("echo:foo")
                .Send("bar")
                    .AssertReply((activity) => Assert.Equal(ActivityTypes.Typing, activity.Type))
                    .AssertReply("echo:bar")
                .StartTestAsync();

            // Assert
            Assert.NotNull(mockHttpContextAccessor.Object.HttpContext.Items);
            Assert.Single(mockHttpContextAccessor.Object.HttpContext.Items);
            Assert.Equal(6, mockTelemetryClient.Invocations.Count);
        }

        [Fact]
        [Trait("TestCategory", "Telemetry")]
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
                    .AssertReply((activity) => Assert.Equal(ActivityTypes.Typing, activity.Type))
                    .AssertReply("echo:foo")
                .Send("bar")
                    .AssertReply((activity) => Assert.Equal(ActivityTypes.Typing, activity.Type))
                    .AssertReply("echo:bar")
                .StartTestAsync();

            // Assert
            Assert.NotNull(mockHttpContextAccessor.Object.HttpContext.Items);
            Assert.Single(mockHttpContextAccessor.Object.HttpContext.Items);
            Assert.Empty(mockTelemetryClient.Invocations);
        }

        [Fact]
        [Trait("TestCategory", "Telemetry")]
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
                    .AssertReply((activity) => Assert.Equal(ActivityTypes.Typing, activity.Type))
                    .AssertReply("echo:foo")
                .Send("bar")
                    .AssertReply((activity) => Assert.Equal(ActivityTypes.Typing, activity.Type))
                    .AssertReply("echo:bar")
                .StartTestAsync();

            // Assert
            Assert.Null(mockHttpContextAccessor.Object.HttpContext);
            Assert.Empty(mockTelemetryClient.Invocations);
        }
    }
}
