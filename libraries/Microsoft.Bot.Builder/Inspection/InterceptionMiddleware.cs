// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.Bot.Builder
{
    public abstract class InterceptionMiddleware : IMiddleware
    {
        internal InterceptionMiddleware(ILogger logger)
        {
            Logger = logger ?? NullLogger.Instance;
        }

        protected ILogger Logger { get; private set; }

        async Task IMiddleware.OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken)
        {
            var (shouldForwardToApplication, shouldIntercept) = await InvokeInboundAsync(turnContext, turnContext.Activity.TraceActivity("ReceivedActivity", "Received Activity"), cancellationToken).ConfigureAwait(false);

            if (shouldIntercept)
            {
                turnContext.OnSendActivities(async (ctx, activities, nextSend) =>
                {
                    var traceActivities = activities.Select(a => a.TraceActivity("SentActivity", "Sent Activity"));
                    await InvokeOutboundAsync(ctx, traceActivities, cancellationToken).ConfigureAwait(false);
                    return await nextSend().ConfigureAwait(false);
                });

                turnContext.OnUpdateActivity(async (ctx, activity, nextUpdate) =>
                {
                    var traceActivity = activity.TraceActivity("MessageUpdate", "Updated Message");
                    await InvokeOutboundAsync(ctx, traceActivity, cancellationToken).ConfigureAwait(false);
                    return await nextUpdate().ConfigureAwait(false);
                });

                turnContext.OnDeleteActivity(async (ctx, reference, nextDelete) =>
                {
                    var traceActivity = reference.TraceActivity();
                    await InvokeOutboundAsync(ctx, traceActivity, cancellationToken).ConfigureAwait(false);
                    await nextDelete().ConfigureAwait(false);
                });

                await InvokeTraceStateAsync(turnContext, cancellationToken).ConfigureAwait(false);
            }

            if (shouldForwardToApplication)
            {
                try
                {
                    await next(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    await InvokeTraceExceptionAsync(turnContext, e.TraceActivity(), cancellationToken).ConfigureAwait(false);
                    throw e;
                }
            }
        }

        protected abstract Task<(bool shouldForwardToApplication, bool shouldIntercept)> InboundAsync(ITurnContext turnContext, Activity traceActivity, CancellationToken cancellationToken);

        protected abstract Task OutboundAsync(ITurnContext turnContext, IEnumerable<Activity> clonedActivities, CancellationToken cancellationToken);

        protected abstract Task TraceStateAsync(ITurnContext turnContext, CancellationToken cancellationToken);

        private async Task<(bool shouldForwardToApplication, bool shouldIntercept)> InvokeInboundAsync(ITurnContext turnContext, Activity traceActivity, CancellationToken cancellationToken)
        {
            try
            {
                return await InboundAsync(turnContext, traceActivity, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Logger.LogWarning($"Exception in inbound interception {e.Message}");
                return (true, false);
            }
        }

        private async Task InvokeOutboundAsync(ITurnContext turnContext, IEnumerable<Activity> traceActivities, CancellationToken cancellationToken)
        {
            try
            {
                await OutboundAsync(turnContext, traceActivities, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Logger.LogWarning($"Exception in outbound interception {e.Message}");
            }
        }

        private Task InvokeOutboundAsync(ITurnContext turnContext, Activity traceActivity, CancellationToken cancellationToken)
        {
            return InvokeOutboundAsync(turnContext, new Activity[] { traceActivity }, cancellationToken);
        }

        private async Task InvokeTraceStateAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            try
            {
                await TraceStateAsync(turnContext, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Logger.LogWarning($"Exception in state interception {e.Message}");
            }
        }

        private async Task InvokeTraceExceptionAsync(ITurnContext turnContext, Activity traceActivity, CancellationToken cancellationToken)
        {
            try
            {
                await OutboundAsync(turnContext, new Activity[] { traceActivity }, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Logger.LogWarning($"Exception in exception interception {e.Message}");
            }
        }
    }
}
