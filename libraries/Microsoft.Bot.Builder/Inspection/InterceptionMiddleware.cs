// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.Bot.Builder
{
    public abstract class InterceptionMiddleware : IMiddleware
    {
        public InterceptionMiddleware(ILogger logger)
        {
            Logger = logger ?? NullLogger.Instance;
        }

        protected ILogger Logger { get; private set; }

        async Task IMiddleware.OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken)
        {
            var (shouldForwardToApplication, shouldIntercept) = await InboundAsync(turnContext, cancellationToken).ConfigureAwait(false);

            if (shouldIntercept)
            {
                turnContext.OnSendActivities(async (ctx, activities, nextSend) =>
                {
                    await OutboundAsync(ctx, activities.Clone(), cancellationToken).ConfigureAwait(false);
                    return await nextSend().ConfigureAwait(false);
                });

                turnContext.OnUpdateActivity(async (ctx, activity, nextUpdate) =>
                {
                    var traceActivity = (Activity)Activity.CreateTraceActivity($"Update", value: activity);
                    await InvokeOutboundAsync(ctx, traceActivity, cancellationToken).ConfigureAwait(false);
                    return await nextUpdate().ConfigureAwait(false);
                });

                turnContext.OnDeleteActivity(async (ctx, reference, nextDelete) =>
                {
                    var traceActivity = (Activity)Activity.CreateTraceActivity($"Delete", value: reference);
                    await InvokeOutboundAsync(ctx, traceActivity, cancellationToken).ConfigureAwait(false);
                    await nextDelete().ConfigureAwait(false);
                });

                await InvokeTraceStateAsync(turnContext, cancellationToken).ConfigureAwait(false);
            }

            if (shouldForwardToApplication)
            {
                await next(cancellationToken).ConfigureAwait(false);
            }
        }

        protected abstract Task<(bool shouldForwardToApplication, bool shouldIntercept)> InboundAsync(ITurnContext turnContext, CancellationToken cancellationToken);

        protected abstract Task OutboundAsync(ITurnContext turnContext, IEnumerable<Activity> clonedActivities, CancellationToken cancellationToken);

        protected abstract Task TraceStateAsync(ITurnContext turnContext, CancellationToken cancellationToken);

        private async Task<(bool shouldForwardToApplication, bool shouldIntercept)> InvokeInboundAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            try
            {
                return await InboundAsync(turnContext, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Logger.LogWarning($"Exception in inbound interception {e.Message}");
                return (true, false);
            }
        }

        private async Task InvokeOutboundAsync(ITurnContext turnContext, IEnumerable<Activity> clonedActivities, CancellationToken cancellationToken)
        {
            try
            {
                await OutboundAsync(turnContext, clonedActivities, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Logger.LogWarning($"Exception in outbound interception {e.Message}");
            }
        }

        private Task InvokeOutboundAsync(ITurnContext turnContext, Activity clonedActivity, CancellationToken cancellationToken)
        {
            return InvokeOutboundAsync(turnContext, new Activity[] { clonedActivity }, cancellationToken);
        }

        private async Task InvokeTraceStateAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            try
            {
                await TraceStateAsync(turnContext, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Logger.LogWarning($"Exception in inbound interception {e.Message}");
            }
        }
    }
}
