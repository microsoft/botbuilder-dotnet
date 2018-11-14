// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.ApplicationInsights;
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
        public void TrackEventTest()
        {
            var telemetryClient = new TelemetryClient();
            var client = new BotTelemetryClient(telemetryClient);
            var eventTelemetry = new EventTelemetry();
            client.TrackEvent(eventTelemetry);
        }

        [TestMethod]
        public void TrackDependencyTest()
        {
            var telemetryClient = new TelemetryClient();
            var client = new BotTelemetryClient(telemetryClient);
            var telemetry = new DependencyTelemetry("my dependency", "my target", "foo", "data");
            client.TrackDependency(telemetry);
        }

        [TestMethod]
        public void TrackExceptionTest()
        {
            var telemetryClient = new TelemetryClient();
            var client = new BotTelemetryClient(telemetryClient);
            var telemetry = new ExceptionTelemetry();
            client.TrackException(telemetry);
        }

        [TestMethod]
        public void TrackTraceTest()
        {
            var telemetryClient = new TelemetryClient();
            var client = new BotTelemetryClient(telemetryClient);
            var telemetry = new TraceTelemetry();
            client.TrackTrace(telemetry);
        }


    }
}
