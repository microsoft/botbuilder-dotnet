// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Microsoft.Bot.Builder.ApplicationInsights
{
    public class BotTelemetryClient : IBotTelemetryClient
    {
        private readonly TelemetryClient _telemetryClient;

        public BotTelemetryClient(TelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
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
        public virtual void TrackAvailability(string name, DateTimeOffset timeStamp, TimeSpan duration, string runLocation, bool success, string message = null, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            var telemetry = new AvailabilityTelemetry(name, timeStamp, duration, runLocation, success, message);
            if (properties != null)
            {
                foreach (var pair in properties)
                {
                    telemetry.Properties.Add(pair.Key, pair.Value);
                }
            }

            if (metrics != null)
            {
                foreach (var pair in metrics)
                {
                    telemetry.Metrics.Add(pair.Key, pair.Value);
                }
            }

            _telemetryClient.TrackAvailability(telemetry);
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
        public virtual void TrackDependency(string dependencyTypeName, string target, string dependencyName, string data, DateTimeOffset startTime, TimeSpan duration, string resultCode, bool success)
        {
            var telemetry = new DependencyTelemetry
            {
                Type = dependencyTypeName,
                Target = target,
                Name = dependencyName,
                Data = data,
                Timestamp = startTime,
                Duration = duration,
                ResultCode = resultCode,
                Success = success,
            };

            _telemetryClient.TrackDependency(telemetry);
        }

        /// <summary>
        /// Logs custom events with extensible named fields.
        /// </summary>
        /// <param name="eventName">A name for the event.</param>
        /// <param name="properties">Named string values you can use to search and classify events.</param>
        /// <param name="metrics">Measurements associated with this event.</param>
        public virtual void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            var telemetry = new EventTelemetry(eventName);
            if (properties != null)
            {
                foreach (var pair in properties)
                {
                    telemetry.Properties.Add(pair.Key, pair.Value);
                }
            }

            if (metrics != null)
            {
                foreach (var pair in metrics)
                {
                    telemetry.Metrics.Add(pair.Key, pair.Value);
                }
            }

            _telemetryClient.TrackEvent(telemetry);
        }

        /// <summary>
        /// Logs a system exception.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="properties">Named string values you can use to classify and search for this exception.</param>
        /// <param name="metrics">Additional values associated with this exception.</param>
        public virtual void TrackException(Exception exception, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            var telemetry = new ExceptionTelemetry(exception);
            if (properties != null)
            {
                foreach (var pair in properties)
                {
                    telemetry.Properties.Add(pair.Key, pair.Value);
                }
            }

            if (metrics != null)
            {
                foreach (var pair in metrics)
                {
                    telemetry.Metrics.Add(pair.Key, pair.Value);
                }
            }

            _telemetryClient.TrackException(telemetry);
        }

        /// <summary>
        /// Send a trace message.
        /// </summary>
        /// <param name="message">Message to display.</param>
        /// <param name="severityLevel">Trace severity level <see cref="Severity"/>.</param>
        /// <param name="properties">Named string values you can use to search and classify events.</param>
        public virtual void TrackTrace(string message, Severity severityLevel, IDictionary<string, string> properties)
        {
            var telemetry = new TraceTelemetry(message)
            {
                SeverityLevel = (SeverityLevel)severityLevel,
            };

            if (properties != null)
            {
                foreach (var pair in properties)
                {
                    telemetry.Properties.Add(pair.Key, pair.Value);
                }
            }

            _telemetryClient.TrackTrace(telemetry);
        }

        /// <summary>
        /// Flushes the in-memory buffer and any metrics being pre-aggregated.
        /// </summary>
        public virtual void Flush() => _telemetryClient.Flush();
    }
}
