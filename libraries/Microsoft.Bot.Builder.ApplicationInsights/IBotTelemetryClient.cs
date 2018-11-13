using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.ApplicationInsights
{
    /// <summary>
    /// Logging client for Bot Telemetry.
    /// </summary>
    public interface IBotTelemetryClient
    {
        /// <summary>
        /// Send information about external dependency call in the application. Create a
        //     separate Microsoft.ApplicationInsights.DataContracts.DependencyTelemetry instance
        //     for each call to Microsoft.ApplicationInsights.TelemetryClient.TrackDependency(Microsoft.ApplicationInsights.DataContracts.DependencyTelemetry)
        /// </summary>
        /// <param name="telemetry">A </param>
        void TrackDependency(DependencyTelemetry telemetry);


        /// <summary>
        /// Send an Microsoft.ApplicationInsights.DataContracts.EventTelemetry for display
        //     in Diagnostic Search and in the Analytics Portal.
        /// </summary>
        /// <param name="telemetry">An event log item.</param>
        void TrackEvent(EventTelemetry telemetry);

        /// <summary>
        /// Send an Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry for display
        //     in Diagnostic Search. Create a separate Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry
        //     instance for each call to Microsoft.ApplicationInsights.TelemetryClient.TrackException(Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry)
        /// </summary>
        /// <param name="telemetry"></param>
        void TrackException(ExceptionTelemetry telemetry);

        /// <summary>
        ///  Send a trace message for display in Diagnostic Search. Create a separate Microsoft.ApplicationInsights.DataContracts.TraceTelemetry
        //     instance for each call to Microsoft.ApplicationInsights.TelemetryClient.TrackTrace(Microsoft.ApplicationInsights.DataContracts.TraceTelemetry).
        //
        /// </summary>
        /// <param name="telemetry">Message with optional properties.</param>
        void TrackTrace(TraceTelemetry telemetry);

        /// <summary>
        /// Tracks single Waterfall step in Application Insights.
        /// </summary>
        /// <param name="waterfallStepContext">The WaterfallStepContext for this waterfall dialog.</param>
        /// <param name="stepFriendlyName">The friendly name that is logged with the waterfall dialog id.</param>
        void TrackWaterfallStep(WaterfallStepContext waterfallStepContext, string stepFriendlyName = null);

        /// <summary>
        /// Flushes the in-memory buffer and any metrics being pre-aggregated.
        /// </summary>
        void Flush();

    }
}
