// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Integration.Runtime.Settings
{
    /// <summary>
    /// Settings for runtime telemetry.
    /// </summary>
    internal class TelemetrySettings
    {
        /// <summary>
        /// Gets or sets the telemetry instrumentation key.
        /// </summary>
        /// <value>
        /// Telemetry instrumentation key.
        /// </value>
        public string InstrumentationKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to log personal information to the telemetry system.
        /// </summary>
        /// <value>
        /// A value indicating whether to log personal information to the telemetry system.
        /// </value>
        public bool LogPersonalInformation { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to log activities to the telemetry system.
        /// </summary>
        /// <value>
        /// A value indicating whether to log activities to the telemetry system.
        /// </value>
        public bool LogActivities { get; set; } = true;
    }
}
