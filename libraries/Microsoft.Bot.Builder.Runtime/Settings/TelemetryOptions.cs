using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Runtime.Settings
{
    internal class TelemetryOptions
    {
        public bool LogPersonalInformation { get; set; }

        public bool LogActivities { get; set; }
    }
}
