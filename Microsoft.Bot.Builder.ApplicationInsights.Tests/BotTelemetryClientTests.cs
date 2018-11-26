// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.ApplicationInsights;
using System.Collections.Generic;
using System;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Channel;
using Moq;
using Microsoft.ApplicationInsights.DataContracts;

namespace Microsoft.Bot.Builder.ApplicationInsights.Tests
{
    [TestClass]
    public class BotTelemetryClientTests
    {

        [TestMethod]
        public void Construct()
        {
            var telemetryClient = new TelemetryClient();
            var client = new BotTelemetryClient(telemetryClient);
            Assert.IsNotNull(client);

        }
        
        [TestMethod]
        public void TrackAvailabilityTest()
        {
            var configuration = new TelemetryConfiguration();
            var sentItems = new List<ITelemetry>();
            var mockTelemetryChannel = new Mock<ITelemetryChannel>();
            mockTelemetryChannel.Setup(c => c.Send(It.IsAny<ITelemetry>()))
                        .Callback<ITelemetry>((telemetry) => sentItems.Add(telemetry))
                        .Verifiable();
            configuration.TelemetryChannel = mockTelemetryChannel.Object;
            configuration.InstrumentationKey = Guid.NewGuid().ToString();
            // configuration.TelemetryInitializers.Add();
            var telemetryClient = new TelemetryClient(configuration);
            var client = new BotTelemetryClient(telemetryClient);

            client.TrackAvailability("test", DateTimeOffset.Now, new TimeSpan(1000), "run location", true,
                "message", new Dictionary<string, string>() { { "hello", "value" } }, new Dictionary<string, double>() { { "metric", 0.6 } });
            
            Assert.IsTrue(sentItems.Count == 1);
            var telem = sentItems[0] as AvailabilityTelemetry;
            Assert.IsTrue(telem != null);
            Assert.IsTrue(telem.Properties["hello"] == "value");
            Assert.IsTrue(telem.Metrics["metric"] == 0.6);
        }


        [TestMethod]
        public void TrackEventTest()
        {
            var configuration = new TelemetryConfiguration();
            var sentItems = new List<ITelemetry>();
            var mockTelemetryChannel = new Mock<ITelemetryChannel>();
            mockTelemetryChannel.Setup(c => c.Send(It.IsAny<ITelemetry>()))
                        .Callback<ITelemetry>((telemetry) => sentItems.Add(telemetry))
                        .Verifiable();
            configuration.TelemetryChannel = mockTelemetryChannel.Object;
            configuration.InstrumentationKey = Guid.NewGuid().ToString();
            // configuration.TelemetryInitializers.Add();
            var telemetryClient = new TelemetryClient(configuration);
            var client = new BotTelemetryClient(telemetryClient);

            client.TrackEvent("test", new Dictionary<string, string>() { { "hello", "value" } }, new Dictionary<string, double>() { { "metric", 0.6 } });

            Assert.IsTrue(sentItems.Count == 1);
            var telem = sentItems[0] as EventTelemetry;
            Assert.IsTrue(telem != null);
            Assert.IsTrue(telem.Properties["hello"] == "value");
            Assert.IsTrue(telem.Metrics["metric"] == 0.6);

        }

        [TestMethod]
        public void TrackDependencyTest()
        {
            var configuration = new TelemetryConfiguration();
            var sentItems = new List<ITelemetry>();
            var mockTelemetryChannel = new Mock<ITelemetryChannel>();
            mockTelemetryChannel.Setup(c => c.Send(It.IsAny<ITelemetry>()))
                        .Callback<ITelemetry>((telemetry) => sentItems.Add(telemetry))
                        .Verifiable();
            configuration.TelemetryChannel = mockTelemetryChannel.Object;
            configuration.InstrumentationKey = Guid.NewGuid().ToString();
            // configuration.TelemetryInitializers.Add();
            var telemetryClient = new TelemetryClient(configuration);
            var client = new BotTelemetryClient(telemetryClient);
            var dateTime = DateTimeOffset.Now;
            var timeSpan = new TimeSpan(12345);
            var dependencyName = "MyDependencyName";
            var resultCode = "myResultCode";
            client.TrackDependency("test", "target", dependencyName, "data", dateTime, timeSpan, resultCode, false );

            Assert.IsTrue(sentItems.Count == 1);
            var telem = sentItems[0] as DependencyTelemetry;
            Assert.IsTrue(telem != null);

            Assert.IsTrue(telem.Timestamp.Ticks == dateTime.Ticks);
            Assert.IsTrue(telem.Duration.Ticks == timeSpan.Ticks);
            Assert.IsTrue(telem.ResultCode == resultCode);
            Assert.IsTrue(telem.Name == dependencyName);
            Assert.IsTrue(telem.Success == false);

        }

        [TestMethod]
        public void TrackExceptionTest()
        {
            var configuration = new TelemetryConfiguration();
            var sentItems = new List<ITelemetry>();
            var mockTelemetryChannel = new Mock<ITelemetryChannel>();
            mockTelemetryChannel.Setup(c => c.Send(It.IsAny<ITelemetry>()))
                        .Callback<ITelemetry>((telemetry) => sentItems.Add(telemetry))
                        .Verifiable();
            configuration.TelemetryChannel = mockTelemetryChannel.Object;
            configuration.InstrumentationKey = Guid.NewGuid().ToString();
            // configuration.TelemetryInitializers.Add();
            var telemetryClient = new TelemetryClient(configuration);
            var client = new BotTelemetryClient(telemetryClient);

            client.TrackException(new Exception(), new Dictionary<string, string>() { { "foo", "bar" } }, new Dictionary<string, double>() { { "metric", 0.6 } });

            Assert.IsTrue(sentItems.Count == 1);
            var telem = sentItems[0] as ExceptionTelemetry;
            Assert.IsTrue(telem != null);
            Assert.IsTrue(telem.Properties["foo"] == "bar");
            Assert.IsTrue(telem.Metrics["metric"] == 0.6);

        }

        [TestMethod]
        public void TrackTraceTest()
        {
            var configuration = new TelemetryConfiguration();
            var sentItems = new List<ITelemetry>();
            var mockTelemetryChannel = new Mock<ITelemetryChannel>();
            mockTelemetryChannel.Setup(c => c.Send(It.IsAny<ITelemetry>()))
                        .Callback<ITelemetry>((telemetry) => sentItems.Add(telemetry))
                        .Verifiable();
            configuration.TelemetryChannel = mockTelemetryChannel.Object;
            configuration.InstrumentationKey = Guid.NewGuid().ToString();
            // configuration.TelemetryInitializers.Add();
            var telemetryClient = new TelemetryClient(configuration);
            var client = new BotTelemetryClient(telemetryClient);
            client.TrackTrace("hello", Severity.Critical, new Dictionary<string, string>() { { "foo", "bar" } });

            Assert.IsTrue(sentItems.Count == 1);
            var telem = sentItems[0] as TraceTelemetry;
            Assert.IsTrue(telem != null);
            Assert.IsTrue(telem.Properties["foo"] == "bar");
            Assert.IsTrue(telem.Message == "hello");
            Assert.IsTrue(telem.SeverityLevel == SeverityLevel.Critical);

        }
    }
}
