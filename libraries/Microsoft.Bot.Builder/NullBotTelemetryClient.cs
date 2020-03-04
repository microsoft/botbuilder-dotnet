// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder
{
    public class NullBotTelemetryClient : IBotTelemetryClient, IBotPageViewTelemetryClient
    {
        public static IBotTelemetryClient Instance { get; } = new NullBotTelemetryClient();

        public void Flush()
        {
        }

        public void TrackAvailability(string name, DateTimeOffset timeStamp, TimeSpan duration, string runLocation, bool success, string message = null, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
        }

        public void TrackDependency(string dependencyTypeName, string target, string dependencyName, string data, DateTimeOffset startTime, TimeSpan duration, string resultCode, bool success)
        {
        }

        public void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
        }

        public void TrackException(Exception exception, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
        }

        public void TrackTrace(string message, Severity severityLevel, IDictionary<string, string> properties)
        {
        }

        public void TrackPageView(string dialogName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
        }
    }
}
