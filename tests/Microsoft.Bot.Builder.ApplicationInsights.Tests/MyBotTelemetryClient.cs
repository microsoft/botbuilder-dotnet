// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights;
using Microsoft.Bot.Builder.ApplicationInsights;

namespace Microsoft.Bot.Builder.Integration.ApplicationInsights.Tests
{
    public class MyBotTelemetryClient : BotTelemetryClient
    {
        public MyBotTelemetryClient(TelemetryClient telemetryClient)
            : base(telemetryClient)
        {
        }

        public override void TrackDependency(string dependencyTypeName, string target, string dependencyName, string data, DateTimeOffset startTime, TimeSpan duration, string resultCode, bool success)
        {
            base.TrackDependency(dependencyName, target, dependencyName, data, startTime, duration, resultCode, success);
        }

        public override void TrackAvailability(string name, DateTimeOffset timeStamp, TimeSpan duration, string runLocation, bool success, string message = null, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            base.TrackAvailability(name, timeStamp, duration, runLocation, success, message, properties, metrics);
        }

        public override void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            base.TrackEvent(eventName, properties, metrics);
        }

        public override void TrackException(Exception exception, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            base.TrackException(exception, properties, metrics);
        }

        public override void TrackTrace(string message, Severity severityLevel, IDictionary<string, string> properties)
        {
            base.TrackTrace(message, severityLevel, properties);
        }

        public override void Flush()
        {
            base.Flush();
        }
    }
}
