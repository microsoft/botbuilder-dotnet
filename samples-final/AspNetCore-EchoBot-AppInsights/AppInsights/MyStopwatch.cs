using System;
using System.Diagnostics;
using Microsoft.ApplicationInsights;

namespace AspNetCore_EchoBot_With_AppInsights.AppInsights
{
    /// <summary>
    /// Sample helper class which logs a Application Insight Dependency (with duration)
    /// For example:
    ///   using (new Stopwatch(telemetryClient, "MyDependency", "Command that triggered this"))
    ///   {
    ///      do some stuff
    ///   }
    /// </summary>
    public class MyStopwatch : IDisposable
    {
        /// <summary>
        /// Creates a sample helper class which can calculate duration of a dependency call.  
        /// </summary>
        /// <param name="telemetryClient">The Application Insights client in which to log the event.</param>
        /// <param name="appInsightDependencyName">Dependency name that will be logged into Application Insights dependency event.</param>
        /// <param name="command">Command used to invoke this dependency.</param>
        public MyStopwatch(TelemetryClient telemetryClient, string appInsightDependencyName, string command)
        {
            _telemetryClient = telemetryClient;
            _appInsightDependencyName = appInsightDependencyName;
            _command = command;
            _startTime = DateTimeOffset.Now;
            _timer = new Stopwatch();
            _timer.Start();
        }

        /// <summary>
        /// Indicates whether or not this dependency operation was successful.
        /// </summary>
        /// <value>Whether the dependency operation was successful.</value>
        public bool Success {get; set;} = true;

        private TelemetryClient _telemetryClient;
        private Stopwatch _timer;
        private readonly string _appInsightDependencyName;
        private readonly string _command;
        private readonly DateTimeOffset _startTime;
        


        public void Dispose()
        {
            _timer.Stop();
            // Log the dependency into Application Insights
            _telemetryClient.TrackDependency(dependencyName: _appInsightDependencyName, commandName: _command, startTime: _startTime, duration: _timer.Elapsed, success: Success);
        }
    }
}
