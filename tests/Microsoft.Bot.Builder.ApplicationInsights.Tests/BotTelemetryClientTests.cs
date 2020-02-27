// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Bot.Builder.ApplicationInsights;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

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

            [TestMethod]
            public void OverrideTest()
            {
                var telemetryClient = new TelemetryClient();
                var botTelemetryClient = new MyBotTelemetryClient(telemetryClient);
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
                _botTelemetryClient.TrackAvailability(
                    "test",
                    DateTimeOffset.Now,
                    new TimeSpan(1000),
                    "run location",
                    true,
                    "message",
                    new Dictionary<string, string>() { { "hello", "value" } },
                    new Dictionary<string, double>() { { "metric", 0.6 } });

                _mockTelemetryChannel.Verify(tc => tc.Send(It.Is<AvailabilityTelemetry>(t => t.Name == "test")));
                _mockTelemetryChannel.Verify(tc => tc.Send(It.Is<AvailabilityTelemetry>(t => t.Message == "message")));
                _mockTelemetryChannel.Verify(tc => tc.Send(It.Is<AvailabilityTelemetry>(t => t.Properties["hello"] == "value")));
                _mockTelemetryChannel.Verify(tc => tc.Send(It.Is<AvailabilityTelemetry>(t => t.Metrics["metric"] == 0.6)));
            }

            [TestMethod]
            public void TrackEventTest()
            {
                _botTelemetryClient.TrackEvent("test", new Dictionary<string, string>() { { "hello", "value" } }, new Dictionary<string, double>() { { "metric", 0.6 } });

                _mockTelemetryChannel.Verify(tc => tc.Send(It.Is<EventTelemetry>(t => t.Name == "test")));
                _mockTelemetryChannel.Verify(tc => tc.Send(It.Is<EventTelemetry>(t => t.Properties["hello"] == "value")));
                _mockTelemetryChannel.Verify(tc => tc.Send(It.Is<EventTelemetry>(t => t.Metrics["metric"] == 0.6)));
            }

            [TestMethod]
            public void TrackDependencyTest()
            {
                _botTelemetryClient.TrackDependency("test", "target", "dependencyname", "data", DateTimeOffset.Now, new TimeSpan(10000), "result", false);

                _mockTelemetryChannel.Verify(tc => tc.Send(It.Is<DependencyTelemetry>(t => t.Type == "test")));
                _mockTelemetryChannel.Verify(tc => tc.Send(It.Is<DependencyTelemetry>(t => t.Target == "target")));
                _mockTelemetryChannel.Verify(tc => tc.Send(It.Is<DependencyTelemetry>(t => t.Name == "dependencyname")));
                _mockTelemetryChannel.Verify(tc => tc.Send(It.Is<DependencyTelemetry>(t => t.Data == "data")));
                _mockTelemetryChannel.Verify(tc => tc.Send(It.Is<DependencyTelemetry>(t => t.ResultCode == "result")));
                _mockTelemetryChannel.Verify(tc => tc.Send(It.Is<DependencyTelemetry>(t => t.Success == false)));
            }

            [TestMethod]
            public void TrackExceptionTest()
            {
                var expectedException = new Exception("test-exception");

                _botTelemetryClient.TrackException(expectedException, new Dictionary<string, string>() { { "foo", "bar" } }, new Dictionary<string, double>() { { "metric", 0.6 } });
                _mockTelemetryChannel.Verify(tc => tc.Send(It.Is<ExceptionTelemetry>(t => t.Exception == expectedException)));
                _mockTelemetryChannel.Verify(tc => tc.Send(It.Is<ExceptionTelemetry>(t => t.Properties["foo"] == "bar")));
                _mockTelemetryChannel.Verify(tc => tc.Send(It.Is<ExceptionTelemetry>(t => t.Metrics["metric"] == 0.6)));
            }

            [TestMethod]
            public void TrackTraceTest()
            {
                _botTelemetryClient.TrackTrace("hello", Severity.Critical, new Dictionary<string, string>() { { "foo", "bar" } });

                _mockTelemetryChannel.Verify(tc => tc.Send(It.Is<TraceTelemetry>(t => t.Message == "hello")));
                _mockTelemetryChannel.Verify(tc => tc.Send(It.Is<TraceTelemetry>(t => t.SeverityLevel == SeverityLevel.Critical)));
                _mockTelemetryChannel.Verify(tc => tc.Send(It.Is<TraceTelemetry>(t => t.Properties["foo"] == "bar")));
            }
        }
    }
}
