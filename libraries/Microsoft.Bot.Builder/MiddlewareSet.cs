// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public class MiddlewareSet : IMiddleware
    {
        private readonly IList<IMiddleware> _middleware = new List<IMiddleware>();

        public MiddlewareSet Use(IMiddleware middleware)
        {
            BotAssert.MiddlewareNotNull(middleware);
            _middleware.Add(middleware);
            return this;
        }

        /// <summary>
        /// Implementation of the IMiddleware interface.
        /// </summary>
        /// <param name="context">The turn context.</param>
        /// <param name="next">The next middleware component.</param>
        /// <param name="cancellationToken">cancellation token.</param>
        public async Task OnTurn(ITurnContext context, NextDelegate next, CancellationToken cancellationToken)
        {
            await ReceiveActivityInternal(context, null, 0, cancellationToken).ConfigureAwait(false);
            await next(cancellationToken).ConfigureAwait(false);
        }

        public async Task ReceiveActivity(ITurnContext context, CancellationToken cancellationToken)
        {
            await ReceiveActivityInternal(context, null, 0, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Intended to be called from Bot, this method performs exactly the same as the
        /// standard ReceiveActivity, except that it runs a user-defined delegate returns
        /// if all Middlware in the receive pipeline was run.
        /// </summary>
        public async Task ReceiveActivityWithStatus(ITurnContext context, Func<ITurnContext, Task> callback, CancellationToken cancellationToken)
        {
            await ReceiveActivityInternal(context, callback, 0, cancellationToken).ConfigureAwait(false);
        }

        private Task ReceiveActivityInternal(ITurnContext context, Func<ITurnContext, Task> callback, int nextMiddlewareIndex, CancellationToken cancellationToken)
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
                return callback?.Invoke(context) ?? Task.CompletedTask;
            }

            // Get the next piece of middleware
            var nextMiddleware = _middleware[nextMiddlewareIndex];

            // Execute the next middleware passing a closure that will recurse back into this method at the next piece of middlware as the NextDelegate
            return nextMiddleware.OnTurn(
                context,
                (ct) => ReceiveActivityInternal(context, callback, nextMiddlewareIndex + 1, ct),
                cancellationToken);
        }
    }
}
