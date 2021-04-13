// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.ApplicationInsights.AspNetCore.Extensions;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Settings
{
    /// <summary>
    /// Settings for runtime telemetry.
    /// </summary>
    internal class TelemetrySettings
    {
        /// <summary>
        /// Gets the configuration key for <see cref="TelemetrySettings"/>.
        /// </summary>
        /// <value>
        /// Configuration key for <see cref="TelemetrySettings"/>.
        /// </value>
        public static string TelemetrySettingsKey => $"{ConfigurationConstants.RuntimeSettingsKey}:telemetry";

        /// <summary>
        /// Gets or sets the telemetry <see cref="ApplicationInsightsServiceOptions"/>.
        /// </summary>
        /// <value>
        /// <see cref="ApplicationInsightsServiceOptions"/>.
        /// </value>
        public ApplicationInsightsServiceOptions Options { get; set; }

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
