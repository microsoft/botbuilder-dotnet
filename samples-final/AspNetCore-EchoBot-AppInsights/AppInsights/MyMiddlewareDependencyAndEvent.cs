using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.Bot.Builder;

namespace AspNetCore_EchoBot_With_AppInsights.AppInsights
{
    /// <summary>
    /// Sample Middleware component that demonstrates how Application Insights can log
    /// custom components.
    /// </summary>
    public class MyMiddlewareDependencyAndEvent : IMiddleware
    {
        public MyMiddlewareDependencyAndEvent()
        {
        }

        public async Task OnTurnAsync(ITurnContext context, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (context.Services.TryGetValue(MyAppInsightsLoggerMiddleware.AppInsightsServiceKey, out var telemetryClient))
            {
                var client = (TelemetryClient)telemetryClient;
                using (new MyStopwatch(telemetryClient: client, appInsightDependencyName: "MyMiddlewareDependency", command: "Sleep"))
                {
                    client.TrackEvent("MyEvent");
                    Thread.Sleep(20);
                }
            }
            if (next != null)
            {
                await next(cancellationToken).ConfigureAwait(false);
            }

        }
    }
    /// <summary>
    /// The Application Insights property names that we're logging.
    /// </summary>
    public static class MyCustomConstants
    {
        public const string ActivityIdProperty = "ActivityId";
        public const string ConversationIdProperty = "ConversationId";
        public const string SleepDuration = "SleepDuration";
    }
}
