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

        /// <summary>
        /// Determines how long the turn should sleep (in ms)
        /// </summary>
        /// <value>The number of ms the middleware should wait.</value>
        public int SleepDuration { get; set; } = 20;

        /// <summary>
        /// Sample middleware which sleeps.
        /// </summary>
        /// <param name="context">Context object containing information for a single turn of conversation with a user.</param>
        /// <param name="next">Used to invoke the next stage of the Middleware pipeline.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task OnTurnAsync(ITurnContext context, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (context.TurnState.TryGetValue(MyAppInsightsLoggerMiddleware.AppInsightsServiceKey, out var telemetryClient))
            {
                var client = (TelemetryClient)telemetryClient;
                using (new MyStopwatchDependency(telemetryClient: client, appInsightDependencyName: "MyMiddlewareDependency", command: "Sleep"))
                {
                    // Add the intent score and conversation id properties
                    var telemetryProperties = new Dictionary<string, string>()
                    {
                        {  MyCustomConstants.ActivityIdProperty, context.Activity.Id },
                    };

                    var conversationId = context.Activity.Conversation.Id;
                    if (!string.IsNullOrEmpty(conversationId))
                    {
                        telemetryProperties.Add(MyQnaConstants.ConversationIdProperty, conversationId);
                    }

                    var telemetryMetrics = new Dictionary<string, double>()
                    {
                        {  MyCustomConstants.SleepDuration, (double)SleepDuration },
                    };

                    client.TrackEvent("MyEvent", telemetryProperties, telemetryMetrics);
                    Thread.Sleep(SleepDuration);
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
