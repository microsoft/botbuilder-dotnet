// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Runtime.Plugins;

namespace Microsoft.Bot.Builder.Runtime.Settings
{
    internal class RuntimeSettings
    {
        public FeatureSettings Features { get; set; }

        public ResourcesSettings Resources { get; set; }        

        public TelemetrySettings Telemetry { get; set; }

        public SkillSettings Skills { get; set; }

        public BotPluginDefinition[] Plugins { get; set; }
    }
}
