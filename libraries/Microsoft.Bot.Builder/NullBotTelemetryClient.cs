// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// A null bot telemtry client that implements <see cref="IBotTelemetryClient"/>.
    /// </summary>
    public class NullBotTelemetryClient : IBotTelemetryClient, IBotPageViewTelemetryClient
    {
        /// <summary>
        /// Gets a new instance of NullBotTelemetryClient.
        /// </summary>
        /// <value>
        /// A new instance of NullBotTelemetryClient.
        /// </value>
        public static IBotTelemetryClient Instance { get; } = new NullBotTelemetryClient();

        /// <summary>
        /// Flushes the in-memory buffer and any metrics being pre-aggregated.
        /// </summary>
        public void Flush()
        {
        }

        /// <summary>
        /// Send information about availability of an application.
        /// </summary>
        /// <param name="name">Availability test name.</param>
        /// <param name="timeStamp">The time when the availability was captured.</param>
        /// <param name="duration">The time taken for the availability test to run.</param>
        /// <param name="runLocation">Name of the location the availability test was run from.</param>
        /// <param name="success">True if the availability test ran successfully.</param>
        /// <param name="message">Error message on availability test run failure.</param>
        /// <param name="properties">Named string values you can use to classify and search for this availability telemetry.</param>
        /// <param name="metrics">Additional values associated with this availability telemetry.</param>
        public void TrackAvailability(string name, DateTimeOffset timeStamp, TimeSpan duration, string runLocation, bool success, string message = null, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
        }

        /// <summary>
        /// Send information about an external dependency (outgoing call) in the application.
        /// </summary>
        /// <param name="dependencyTypeName">Name of the command initiated with this dependency call. Low cardinality value.
        /// Examples are SQL, Azure table, and HTTP.</param>
        /// <param name="target">External dependency target.</param>
        /// <param name="dependencyName">Name of the command initiated with this dependency call. Low cardinality value.
        /// Examples are stored procedure name and URL path template.</param>
        /// <param name="data">Command initiated by this dependency call. Examples are SQL statement and HTTP
        /// URL's with all query parameters.</param>
        /// <param name="startTime">The time when the dependency was called.</param>
        /// <param name="duration">The time taken by the external dependency to handle the call.</param>
        /// <param name="resultCode">Result code of dependency call execution.</param>
        /// <param name="success">True if the dependency call was handled successfully.</param>
        public void TrackDependency(string dependencyTypeName, string target, string dependencyName, string data, DateTimeOffset startTime, TimeSpan duration, string resultCode, bool success)
        {
        }

        /// <summary>
        /// Logs custom events with extensible named fields.
        /// </summary>
        /// <param name="eventName">A name for the event.</param>
        /// <param name="properties">Named string values you can use to search and classify events.</param>
        /// <param name="metrics">Measurements associated with this event.</param>
        public void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
        }

        /// <summary>
        /// Logs a system exception.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="properties">Named string values you can use to classify and search for this exception.</param>
        /// <param name="metrics">Additional values associated with this exception.</param>
        public void TrackException(Exception exception, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
        }

        /// <summary>
        /// Send a trace message.
        /// </summary>
        /// <param name="message">Message to display.</param>
        /// <param name="severityLevel">Trace severity level <see cref="Severity"/>.</param>
        /// <param name="properties">Named string values you can use to search and classify events.</param>
        public void TrackTrace(string message, Severity severityLevel, IDictionary<string, string> properties)
        {
        }

        /// <summary>
        /// Logs an Application Insights page view.
        /// </summary>
        /// <param name="dialogName">The name of the dialog to log the entry / start for.</param>
        /// <param name="properties">Named string values you can use to search and classify events.</param>
        /// <param name="metrics">Measurements associated with this event.</param>
        public void TrackPageView(string dialogName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
        }
    }
}
