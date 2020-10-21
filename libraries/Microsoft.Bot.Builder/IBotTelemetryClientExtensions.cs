// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Extension methods for <see cref="IBotTelemetryClient"/>.
    /// </summary>
    public static class IBotTelemetryClientExtensions
    {
        /// <summary>
        /// Log a DialogView using the TrackPageView method on the IBotTelemetryClient if IBotPageViewTelemetryClient has been implemented.
        /// Alternatively log the information out via TrackTrace.
        /// </summary>
        /// <param name="telemetryClient">The TelemetryClient that implements IBotTelemetryClient.</param>
        /// <param name="dialogName">The name of the dialog to log the entry / start for.</param>
        /// <param name="properties">Named string values you can use to search and classify events.</param>
        /// <param name="metrics">Measurements associated with this event.</param>
        public static void TrackDialogView(this IBotTelemetryClient telemetryClient, string dialogName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            if (telemetryClient is IBotPageViewTelemetryClient pageViewClient)
            {
                pageViewClient.TrackPageView(dialogName, properties, metrics);
            }
            else
            {
                telemetryClient.TrackTrace($"Dialog View: {dialogName}", Severity.Information, properties);
            }
        }
    }
}
