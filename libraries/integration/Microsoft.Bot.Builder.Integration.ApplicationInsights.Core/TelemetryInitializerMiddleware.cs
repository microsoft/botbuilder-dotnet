// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Integration.ApplicationInsights.Core
{
    /// <summary>
    /// Middleware for storing incoming activity on the HttpContext to make it available to the <see cref="TelemetryBotIdInitializer"/>.
    /// </summary>
    public class TelemetryInitializerMiddleware : IMiddleware
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly TelemetryLoggerMiddleware _telemetryLoggerMiddleware;
        private readonly bool _logActivityTelemetry;

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryInitializerMiddleware"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">The IHttpContextAccessor to allow access to the HttpContext.</param>
        /// <param name="telemetryLoggerMiddleware">The TelemetryLoggerMiddleware to allow for logging of activity events.</param>
        /// <param name="logActivityTelemetry">Indicates if the TelemetryLoggerMiddleware should be executed to log activity events.</param>
        public TelemetryInitializerMiddleware(IHttpContextAccessor httpContextAccessor, TelemetryLoggerMiddleware telemetryLoggerMiddleware, bool logActivityTelemetry = true)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _telemetryLoggerMiddleware = telemetryLoggerMiddleware;
            _logActivityTelemetry = logActivityTelemetry;
        }

        /// <summary>
        /// Stores the incoming activity as JSON in the items collection on the HttpContext.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="nextDelegate">The delegate to call to continue the bot middleware pipeline.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <seealso cref="ITurnContext"/>
        /// <seealso cref="Bot.Schema.IActivity"/>
        public virtual async Task OnTurnAsync(ITurnContext turnContext, NextDelegate nextDelegate, CancellationToken cancellationToken)
        {
            BotAssert.ContextNotNull(turnContext);

            if (turnContext.Activity != null)
            {
                var activity = turnContext.Activity;

                var httpContext = _httpContextAccessor.HttpContext;
                var items = httpContext?.Items;

                var activityJson = JObject.FromObject(activity);

                if (items != null && items.ContainsKey(TelemetryBotIdInitializer.BotActivityKey))
                {
                    items.Remove(TelemetryBotIdInitializer.BotActivityKey);
                }

                items?.Add(TelemetryBotIdInitializer.BotActivityKey, activityJson);
            }

            if (_logActivityTelemetry)
            {
                await _telemetryLoggerMiddleware
                    .OnTurnAsync(turnContext, nextDelegate, cancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                await nextDelegate(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
