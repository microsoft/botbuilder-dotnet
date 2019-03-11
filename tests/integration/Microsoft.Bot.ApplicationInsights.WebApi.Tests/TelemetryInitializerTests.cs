// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Bot.Builder.Integration.ApplicationInsights.WebApi;
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
            var httpContext = new HttpContext(
                    new HttpRequest(string.Empty, "http://google.com", string.Empty),
                    new HttpResponse(new StringWriter()));
            HttpContext.Current = httpContext;

            // Simulate what Middleware does to read body
            var fromID = "FROMID";
            var channelID = "CHANNELID";
            var conversationID = "CONVERSATIONID";
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

            Assert.IsTrue(sentItems.Count == 1);
            var telem = sentItems[0] as EventTelemetry;
            Assert.IsTrue(telem != null);
            Assert.IsTrue(telem.Properties["activityId"] == activityID);
            Assert.IsTrue(telem.Properties["activityType"] == "message");
            Assert.IsTrue(telem.Properties["channelId"] == "CHANNELID");
            Assert.IsTrue(telem.Context.Session.Id == conversationID);
            Assert.IsTrue(telem.Context.User.Id == channelID + fromID);
            Assert.IsTrue(telem.Properties["hello"] == "value");
            Assert.IsTrue(telem.Metrics["metric"] == 0.6);
        }

        [TestMethod]
        public void VerifyTracePropoerties()
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

            Assert.IsTrue(sentItems.Count == 1);
            var telem = sentItems[0] as TraceTelemetry;
            Assert.IsTrue(telem != null);
            Assert.IsTrue(telem.Properties["activityId"] == activityID);
            Assert.IsTrue(telem.Properties["activityType"] == "message");
            Assert.IsTrue(telem.Properties["channelId"] == "CHANNELID");
            Assert.IsTrue(telem.Context.Session.Id == conversationID);
            Assert.IsTrue(telem.Context.User.Id == channelID + fromID);
        }

        [TestMethod]
        public void VerifyRequestPropoerties()
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

            Assert.IsTrue(sentItems.Count == 1);
            var telem = sentItems[0] as RequestTelemetry;
            Assert.IsTrue(telem != null);
            Assert.IsTrue(telem.Properties["activityId"] == activityID);
            Assert.IsTrue(telem.Properties["activityType"] == "message");
            Assert.IsTrue(telem.Properties["channelId"] == "CHANNELID");
            Assert.IsTrue(telem.Context.Session.Id == conversationID);
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

            Assert.IsTrue(sentItems.Count == 1);
            var telem = sentItems[0] as EventTelemetry;
            var properties = (ISupportProperties)telem;
            Assert.IsTrue(telem != null);
            Assert.IsTrue(properties.Properties["activityId"] == activityID);
            Assert.IsTrue(properties.Properties["activityType"] == "message");
            Assert.IsTrue(telem.Properties["channelId"] == "CHANNELID");
            Assert.IsTrue(telem.Context.User.Id == channelID + fromID);
            Assert.IsTrue(properties.Properties["hello"] == "value");
            Assert.IsTrue(telem.Metrics["metric"] == 0.6);
        }
    }
}
