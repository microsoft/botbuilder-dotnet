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
    /// <summary>
    /// Middleware for the interception of activities.
    /// </summary>
    public abstract class InterceptionMiddleware : IMiddleware
    {
        internal InterceptionMiddleware(ILogger logger)
        {
            Logger = logger ?? NullLogger.Instance;
        }

        /// <summary>
        /// Gets the logger for the current object.
        /// </summary>
        /// <value>
        /// The logger for the current object.
        /// </value>
        protected ILogger Logger { get; }

        async Task IMiddleware.OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken)
        {
            var (shouldForwardToApplication, shouldIntercept) = await InvokeInboundAsync(turnContext, turnContext.Activity.TraceActivity("ReceivedActivity", "Received Activity"), cancellationToken).ConfigureAwait(false);

            if (shouldIntercept)
            {
                turnContext.OnSendActivities(async (ctx, activities, nextSend) =>
                {
                    var traceActivities = activities.Select(a => a.Type == ActivityTypes.Trace ? a.CloneTraceActivity() : a.TraceActivity("SentActivity", "Sent Activity"));
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
                    throw;
                }
            }

            if (shouldIntercept)
            {
                await InvokeTraceStateAsync(turnContext, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Overriding methods implement processing of inbound activities.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="traceActivity">The trace activity.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected abstract Task<(bool shouldForwardToApplication, bool shouldIntercept)> InboundAsync(ITurnContext turnContext, Activity traceActivity, CancellationToken cancellationToken);

        /// <summary>
        /// Overriding methods implement processing of outbound activities.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="clonedActivities">A collection of activities.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected abstract Task OutboundAsync(ITurnContext turnContext, IEnumerable<Activity> clonedActivities, CancellationToken cancellationToken);

        /// <summary>
        /// Overriding methods implement processing of state management objects.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected abstract Task TraceStateAsync(ITurnContext turnContext, CancellationToken cancellationToken);

        private async Task<(bool shouldForwardToApplication, bool shouldIntercept)> InvokeInboundAsync(ITurnContext turnContext, Activity traceActivity, CancellationToken cancellationToken)
        {
            try
            {
                return await InboundAsync(turnContext, traceActivity, cancellationToken).ConfigureAwait(false);
            }
#pragma warning disable CA1031 // Do not catch general exception types (exception are logged and ignored)
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                Logger.LogWarning($"Exception in inbound interception {ex.Message}");
                return (true, false);
            }
        }

        private async Task InvokeOutboundAsync(ITurnContext turnContext, IEnumerable<Activity> traceActivities, CancellationToken cancellationToken)
        {
            try
            {
                await OutboundAsync(turnContext, traceActivities, cancellationToken).ConfigureAwait(false);
            }
#pragma warning disable CA1031 // Do not catch general exception types (exception are logged and ignored)
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                Logger.LogWarning($"Exception in outbound interception {ex.Message}");
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
#pragma warning disable CA1031 // Do not catch general exception types (exception are logged and ignored)
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                Logger.LogWarning($"Exception in state interception {ex.Message}");
            }
        }

        private async Task InvokeTraceExceptionAsync(ITurnContext turnContext, Activity traceActivity, CancellationToken cancellationToken)
        {
            try
            {
                await OutboundAsync(turnContext, new Activity[] { traceActivity }, cancellationToken).ConfigureAwait(false);
            }
#pragma warning disable CA1031 // Do not catch general exception types (exception are logged and ignored)
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                Logger.LogWarning($"Exception in exception interception {ex.Message}");
            }
        }
    }
}
