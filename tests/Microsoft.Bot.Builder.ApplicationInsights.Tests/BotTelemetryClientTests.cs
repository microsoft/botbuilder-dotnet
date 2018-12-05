// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using System.Collections.Generic;
using System;
using Microsoft.ApplicationInsights.Extensibility;
using Moq;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.Bot.Builder.ApplicationInsights;

namespace Microsoft.Bot.Builder.Integration.ApplicationInsights.Tests
{
    public class BotTelemetryClientTests
    {
        [TestClass]
        public class ConstructorTests
        {
            [TestMethod]
            public void NullTelemetryClientThrows()
            {
                try
                {
                    new BotTelemetryClient(null);

                    Assert.Fail("Expected an exception to be thrown.");
                }
                catch (ArgumentNullException exception)
                {
                    Assert.AreEqual<string>("telemetryClient", exception.ParamName);
                }
            }

            [TestMethod]
            public void NonNullTelemtryClientSucceeds()
            {
                var telemetryClient = new TelemetryClient();

                var botTelemetryClient = new BotTelemetryClient(telemetryClient);
            }
        }

        [TestClass]
        public class TrackTelemetryTests
        {
            private BotTelemetryClient _botTelemetryClient;
            private Mock<ITelemetryChannel> _mockTelemetryChannel;

            [TestInitialize]
            public void TestInitialize()
            {
                _mockTelemetryChannel = new Mock<ITelemetryChannel>();

                var telemetryConfiguration = new TelemetryConfiguration("UNITTEST-INSTRUMENTATION-KEY", _mockTelemetryChannel.Object);
                var telemetryClient = new TelemetryClient(telemetryConfiguration);

                _botTelemetryClient = new BotTelemetryClient(telemetryClient);
            }

            [TestMethod]
            public void TrackAvailabilityTest()
            {
                _botTelemetryClient.TrackAvailability("test", DateTimeOffset.Now, new TimeSpan(1000), "run location", true,
                    "message", new Dictionary<string, string>() { { "hello", "value" } }, new Dictionary<string, double>() { { "metric", 0.6 } });

                _mockTelemetryChannel.Verify(tc => tc.Send(It.Is<AvailabilityTelemetry>(t =>
                    t.Name == "test"
                        &&
                    t.Message == "message"
                        &&
                    t.Properties["hello"] == "value"
                        &&
                    t.Metrics["metric"] == 0.6)));
            }


            [TestMethod]
            public void TrackEventTest()
            {
                _botTelemetryClient.TrackEvent("test", new Dictionary<string, string>() { { "hello", "value" } }, new Dictionary<string, double>() { { "metric", 0.6 } });

                _mockTelemetryChannel.Verify(tc => tc.Send(It.Is<EventTelemetry>(t =>
                    t.Name == "test"
                        &&
                    t.Properties["hello"] == "value"
                        &&
                    t.Metrics["metric"] == 0.6
                )));
            }

            [TestMethod]
            public void TrackDependencyTest()
            {
                _botTelemetryClient.TrackDependency("test", "target", "dependencyname", "data", DateTimeOffset.Now, new TimeSpan(10000), "result", false);

                _mockTelemetryChannel.Verify(tc => tc.Send(It.Is<DependencyTelemetry>(t =>
                    t.Type == "test"
                        &&
                    t.Target == "target"
                        &&
                    t.Name == "dependencyname"
                        &&
                    t.Data == "data"
                        &&
                    t.ResultCode == "result"
                        &&
                    t.Success == false)));
            }

            [TestMethod]
            public void TrackExceptionTest()
            {
                var expectedException = new Exception("test-exception");

                _botTelemetryClient.TrackException(expectedException, new Dictionary<string, string>() { { "foo", "bar" } }, new Dictionary<string, double>() { { "metric", 0.6 } });

                _mockTelemetryChannel.Verify(tc => tc.Send(It.Is<ExceptionTelemetry>(t =>
                    t.Exception == expectedException
                        &&
                    t.Properties["foo"] == "bar"
                        &&
                    t.Metrics["metric"] == 0.6)));
            }

            [TestMethod]
            public void TrackTraceTest()
            {
                _botTelemetryClient.TrackTrace("hello", Severity.Critical, new Dictionary<string, string>() { { "foo", "bar" } });

                _mockTelemetryChannel.Verify(tc => tc.Send(It.Is<TraceTelemetry>(t =>
                    t.Message == "hello"
                        &&
                    t.SeverityLevel == SeverityLevel.Critical
                        &&
                    t.Properties["foo"] == "bar")));
            }
        }
    }
}
