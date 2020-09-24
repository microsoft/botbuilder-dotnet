// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Bot.Builder.Integration.ApplicationInsights.WebApi;
using Microsoft.Bot.Schema;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.ApplicationInsights.WebApi.Tests
{
    [Trait("TestCategory", "ApplicationInsights")]
    public class TelemetryInitializerTests
    {
        [Fact]
        public void VerifyAllTelemetryProperties()
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
            var httpContext = new HttpContext(
                    new HttpRequest(string.Empty, "http://google.com", string.Empty),
                    new HttpResponse(new StringWriter()));
            HttpContext.Current = httpContext;

            // Simulate what Middleware does to read body
            var fromID = "FROMID";
            var channelID = "CHANNELID";
            var conversationID = "CONVERSATIONID";
            var sessionId = GetHashedConversationId(conversationID);
            var activityID = "ACTIVITYID";
            var activity = Activity.CreateMessageActivity();
            activity.From = new ChannelAccount(fromID);
            activity.ChannelId = channelID;
            activity.Conversation = new ConversationAccount(false, "CONVOTYPE", conversationID);
            activity.Id = activityID;
            var activityBody = JObject.FromObject(activity);
            HttpContext.Current.Items.Add(TelemetryBotIdInitializer.BotActivityKey, activityBody);
            configuration.TelemetryInitializers.Add(new TelemetryBotIdInitializer());
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
            var httpContext = new HttpContext(
                    new HttpRequest(string.Empty, "http://google.com", string.Empty),
                    new HttpResponse(new StringWriter()));
            HttpContext.Current = httpContext;

            // Simulate what Middleware does to read body
            var fromID = "FROMID";
            var channelID = "CHANNELID";
            var conversationID = "CONVERSATIONID";
            var sessionId = GetHashedConversationId(conversationID);
            var activityID = "ACTIVITYID";
            var activity = Activity.CreateMessageActivity();
            activity.From = new ChannelAccount(fromID);
            activity.ChannelId = channelID;
            activity.Conversation = new ConversationAccount(false, "CONVOTYPE", conversationID);
            activity.Id = activityID;
            var activityBody = JObject.FromObject(activity);
            HttpContext.Current.Items.Add(TelemetryBotIdInitializer.BotActivityKey, activityBody);
            configuration.TelemetryInitializers.Add(new TelemetryBotIdInitializer());
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
            var httpContext = new HttpContext(
                    new HttpRequest(string.Empty, "http://google.com", string.Empty),
                    new HttpResponse(new StringWriter()));
            HttpContext.Current = httpContext;

            // Simulate what Middleware does to read body
            var fromID = "FROMID";
            var channelID = "CHANNELID";
            var conversationID = "CONVERSATIONID";
            var sessionId = GetHashedConversationId(conversationID);
            var activityID = "ACTIVITYID";
            var activity = Activity.CreateMessageActivity();
            activity.From = new ChannelAccount(fromID);
            activity.ChannelId = channelID;
            activity.Conversation = new ConversationAccount(false, "CONVOTYPE", conversationID);
            activity.Id = activityID;
            var activityBody = JObject.FromObject(activity);
            HttpContext.Current.Items.Add(TelemetryBotIdInitializer.BotActivityKey, activityBody);
            configuration.TelemetryInitializers.Add(new TelemetryBotIdInitializer());
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
            var httpContext = new HttpContext(
                    new HttpRequest(string.Empty, "http://google.com", string.Empty),
                    new HttpResponse(new StringWriter()));
            HttpContext.Current = httpContext;

            // Simulate what Middleware does to read body
            var fromID = "FROMID";
            var channelID = "CHANNELID";
            var conversationID = "CONVERSATIONID";
            var sessionId = GetHashedConversationId(conversationID);
            var activityID = "ACTIVITYID";
            var activity = Activity.CreateMessageActivity();
            activity.From = new ChannelAccount(fromID);
            activity.ChannelId = channelID;
            activity.Conversation = new ConversationAccount(false, "CONVOTYPE", conversationID);
            activity.Id = activityID;
            var activityBody = JObject.FromObject(activity);
            HttpContext.Current.Items.Add(TelemetryBotIdInitializer.BotActivityKey, activityBody);
            configuration.TelemetryInitializers.Add(new TelemetryBotIdInitializer());
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

            //// Mock http context
            var httpContext = new HttpContext(
                    new HttpRequest(string.Empty, "http://google.com", string.Empty),
                    new HttpResponse(new StringWriter()));
            HttpContext.Current = httpContext;

            // Simulate what Middleware does to read body
            var fromID = "FROMID";
            var channelID = "CHANNELID";
            var activityID = "ACTIVITYID";
            var activity = Activity.CreateMessageActivity();
            activity.From = new ChannelAccount(fromID);
            activity.ChannelId = channelID;
            activity.Id = activityID;
            var activityBody = JObject.FromObject(activity);
            HttpContext.Current.Items.Add(TelemetryBotIdInitializer.BotActivityKey, activityBody);
            configuration.TelemetryInitializers.Add(new TelemetryBotIdInitializer());
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
            Assert.Equal("value", properties.Properties["hello"]);
            Assert.Equal(0.6, telem.Metrics["metric"]);
        }

        private string GetHashedConversationId(string conversationID)
        {
            using (var sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(conversationID));
                return Convert.ToBase64String(bytes);
            }
        }
    }
}
