// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.ApplicationInsights
{
    public class BotTelemetryClient : IBotTelemetryClient
    {
        private readonly TelemetryClient _telemetryClient;

        public BotTelemetryClient(TelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient;
        }
        /// <summary>
        /// Send information about external dependency call in the application. Create a
        //     separate Microsoft.ApplicationInsights.DataContracts.DependencyTelemetry instance
        //     for each call to Microsoft.ApplicationInsights.TelemetryClient.TrackDependency(Microsoft.ApplicationInsights.DataContracts.DependencyTelemetry)
        /// </summary>
        /// <param name="telemetry">A </param>
        public void TrackDependency(DependencyTelemetry telemetry)
        {
            _telemetryClient.TrackDependency(telemetry);
        }


        /// <summary>
        /// Send an Microsoft.ApplicationInsights.DataContracts.EventTelemetry for display
        //     in Diagnostic Search and in the Analytics Portal.
        /// </summary>
        /// <param name="telemetry">An event log item.</param>
        public void TrackEvent(EventTelemetry telemetry)
        {
            _telemetryClient.TrackEvent(telemetry);
        }

        /// <summary>
        /// Send an Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry for display
        //     in Diagnostic Search. Create a separate Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry
        //     instance for each call to Microsoft.ApplicationInsights.TelemetryClient.TrackException(Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry)
        /// </summary>
        /// <param name="telemetry"></param>
        public void TrackException(ExceptionTelemetry telemetry)
        {
            _telemetryClient.TrackException(telemetry);
        }

        /// <summary>
        ///  Send a trace message for display in Diagnostic Search. Create a separate Microsoft.ApplicationInsights.DataContracts.TraceTelemetry
        //     instance for each call to Microsoft.ApplicationInsights.TelemetryClient.TrackTrace(Microsoft.ApplicationInsights.DataContracts.TraceTelemetry).
        //
        /// </summary>
        /// <param name="telemetry">Message with optional properties.</param>
        public void TrackTrace(TraceTelemetry telemetry)
        {
            _telemetryClient.TrackTrace(telemetry);
        }

        /// <summary>
        /// Tracks single Waterfall step in Application Insights.
        /// </summary>
        /// <param name="waterfallStepContext">The WaterfallStepContext for this waterfall dialog.</param>
        /// <param name="stepFriendlyName">The friendly name that is logged with the waterfall dialog id.</param>
        public void TrackWaterfallStep(WaterfallStepContext waterfallStepContext, string stepFriendlyName)
        {
            _telemetryClient.TrackWaterfallStep(waterfallStepContext, stepFriendlyName);
        }

        /// <summary>
        /// Flushes the in-memory buffer and any metrics being pre-aggregated.
        /// </summary>
        public void Flush()
        {

        }

    }
}
