// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Contains an ordered set of <see cref="IMiddleware"/>.
    /// </summary>
    public class MiddlewareSet : IMiddleware
    {
        private readonly IList<IMiddleware> _middleware = new List<IMiddleware>();

        /// <summary>
        /// Adds a middleware object to the end of the set.
        /// </summary>
        /// <param name="middleware">The middleware to add.</param>
        /// <returns>The updated middleware set.</returns>
        /// <see cref="BotAdapter.Use(IMiddleware)"/>
        public MiddlewareSet Use(IMiddleware middleware)
        {
            BotAssert.MiddlewareNotNull(middleware);
            _middleware.Add(middleware);
            return this;
        }

        /// <summary>
        /// Processes an incoming activity.
        /// </summary>
        /// <param name="context">The context object for this turn.</param>
        /// <param name="next">The delegate to call to continue the bot middleware pipeline.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task OnTurnAsync(ITurnContext context, NextDelegate next, CancellationToken cancellationToken)
        {
            await ReceiveActivityInternalAsync(context, null, 0, cancellationToken).ConfigureAwait(false);
            await next(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Processes an activity.
        /// </summary>
        /// <param name="context">The context object for the turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task ReceiveActivityAsync(ITurnContext context, CancellationToken cancellationToken)
        {
            await ReceiveActivityInternalAsync(context, null, 0, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Processes an activity.
        /// </summary>
        /// <param name="context">The context object for the turn.</param>
        /// <param name="callback">The delegate to call when the set finishes processing the activity.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task ReceiveActivityWithStatusAsync(ITurnContext context, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            await ReceiveActivityInternalAsync(context, callback, 0, cancellationToken).ConfigureAwait(false);
        }

        private Task ReceiveActivityInternalAsync(ITurnContext context, BotCallbackHandler callback, int nextMiddlewareIndex, CancellationToken cancellationToken)
        {
            // Check if we're at the end of the middleware list yet
            if (nextMiddlewareIndex == _middleware.Count)
            {
                // If all the Middlware ran, the "leading edge" of the tree is now complete.
                // This means it's time to run any developer specified callback.
                // Once this callback is done, the "trailing edge" calls are then completed. This
                // allows code that looks like:
                //      Trace.TraceInformation("before");
                //      await next();
                //      Trace.TraceInformation("after");
                // to run as expected.

                // If a callback was provided invoke it now and return its task, otherwise just return the completed task
                return callback?.Invoke(context, cancellationToken) ?? Task.CompletedTask;
            }

            // Get the next piece of middleware
            var nextMiddleware = _middleware[nextMiddlewareIndex];

            // Execute the next middleware passing a closure that will recurse back into this method at the next piece of middlware as the NextDelegate
            return nextMiddleware.OnTurnAsync(
                context,
                (ct) => ReceiveActivityInternalAsync(context, callback, nextMiddlewareIndex + 1, ct),
                cancellationToken);
        }
    }
}
