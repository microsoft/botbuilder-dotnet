// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using System.Collections.Generic;
using System;

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
            var telemetryClient = new TelemetryClient();
            var client = new BotTelemetryClient(telemetryClient);

            client.TrackAvailability("test", DateTimeOffset.Now, new TimeSpan(1000), "run location", true,
                "message", new Dictionary<string, string>() { { "hello", "value" } }, new Dictionary<string, double>() { { "metric", 0.6 } });
        }


        [TestMethod]
        public void TrackEventTest()
        {
            var telemetryClient = new TelemetryClient();
            var client = new BotTelemetryClient(telemetryClient);
            
            client.TrackEvent("test", new Dictionary<string, string>() { { "hello", "value" } }, new Dictionary<string, double>() { { "metric", 0.6 } });
        }

        [TestMethod]
        public void TrackDependencyTest()
        {
            var telemetryClient = new TelemetryClient();
            var client = new BotTelemetryClient(telemetryClient);
            client.TrackDependency("test", "target", "dependencyname", "data", DateTimeOffset.Now, new TimeSpan(10000), "result", false );
        }

        [TestMethod]
        public void TrackExceptionTest()
        {
            var telemetryClient = new TelemetryClient();
            var client = new BotTelemetryClient(telemetryClient);
            client.TrackException(new Exception(), new Dictionary<string, string>() { { "foo", "bar" } }, new Dictionary<string, double>() { { "metric", 0.6 } });
        }

        [TestMethod]
        public void TrackTraceTest()
        {
            var telemetryClient = new TelemetryClient();
            var client = new BotTelemetryClient(telemetryClient);
            client.TrackTrace("hello", Severity.Critical, new Dictionary<string, string>() { { "foo", "bar" } });
        }


    }
}
