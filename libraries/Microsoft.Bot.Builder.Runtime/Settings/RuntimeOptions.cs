using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Runtime.Settings
{
    internal class RuntimeOptions
    {
        public RuntimeFeatures Features { get; set; }

        public RuntimeResources Resources { get; set; }

        public TelemetryOptions TelemetryOptions { get; set; }

        public string DefaultLocale { get; set; }
    }
}
