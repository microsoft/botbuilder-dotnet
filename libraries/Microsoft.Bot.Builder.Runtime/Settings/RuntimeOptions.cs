using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Runtime.Plugins;

namespace Microsoft.Bot.Builder.Runtime.Settings
{
    internal class RuntimeOptions
    {
        public RuntimeFeatures Features { get; set; }

        public RuntimeResources Resources { get; set; }

        public IList<BotPluginDefinition> Plugins { get; set; }

        public TelemetryOptions TelemetryOptions { get; set; }

        public string DefaultLocale { get; set; }
    }
}
