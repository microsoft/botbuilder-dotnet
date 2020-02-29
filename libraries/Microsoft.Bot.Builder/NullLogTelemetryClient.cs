using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder
{
    public class NullLogTelemetryClient : LogTelemetryClientBase
    {
        public override void Flush()
        {
        }

        public override void TrackAvailability(string name, DateTimeOffset timeStamp, TimeSpan duration, string runLocation, bool success, string message = null, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
        }

        public override void TrackDependency(string dependencyTypeName, string target, string dependencyName, string data, DateTimeOffset startTime, TimeSpan duration, string resultCode, bool success)
        {
        }

        public override void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
        }

        public override void TrackException(Exception exception, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
        }

        public override void TrackDialogView(string name, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
        }

        public override void TrackTrace(string message, Severity severityLevel, IDictionary<string, string> properties)
        {
        }
    }
}
