using System.Collections.Generic;

namespace Microsoft.Bot.Builder
{
    public static class IBotTelemetryClientExtensions
    {
        /// <summary>
        /// Adds the ability to call TrackPageView on the TelemetryClient if it implements IBotPageViewTelemetryClient as well as IBotTelemetryClient.
        /// </summary>
        /// <param name="telemetryClient">The TelemetryClient that implements IBotTelemetryClient.</param>
        /// <param name="dialogName">The name of the dialog to log the entry / start for.</param>
        /// <param name="properties">Named string values you can use to search and classify events.</param>
        /// <param name="metrics">Measurements associated with this event.</param>
        public static void TrackPageView(this IBotTelemetryClient telemetryClient, string dialogName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            if (telemetryClient is IBotPageViewTelemetryClient pageViewClient)
            {
                pageViewClient.TrackPageView(dialogName, properties, metrics);
            }
            else
            {
                telemetryClient.TrackTrace("TelemetryClient cannot track PageView telemtry as it does not implement IBotPageViewTelemetryClient", Severity.Information, properties);
            }
        }
    }
}
