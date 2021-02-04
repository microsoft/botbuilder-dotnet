// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Runtime.Settings
{
    internal class TelemetrySettings
    {
        public string InstrumentationKey { get; set; }

        public bool LogPersonalInformation { get; set; } = false;

        public bool LogActivities { get; set; } = true;
    }
}
