// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public class MiddlewareSet : IMiddleware
    {
        public delegate Task NextDelegate();

        private readonly IList<IMiddleware> _middleware = new List<IMiddleware>();

        public MiddlewareSet Use(IMiddleware middleware)
        {
            BotAssert.MiddlewareNotNull(middleware);
            _middleware.Add(middleware);
            return this;
        }

        public async Task ReceiveActivity(IBotContext context)
        {
            await ReceiveActivityInternal(context, _middleware, null).ConfigureAwait(false);
        }

        public async Task ReceiveActivity(IBotContext context, NextDelegate next)
        {
            await ReceiveActivityInternal(context, _middleware, null).ConfigureAwait(false);
            await next().ConfigureAwait(false);
        }

        /// <summary>
        /// Intended to be called from Bot, this method performs exactly the same as the
        /// standard ReceiveActivity, except that it runs a user-defined delegate returns 
        /// if all Middlware in the receive pipeline was run.
        /// </summary>
        public async Task ReceiveActivityWithStatus(IBotContext context, Func<IBotContext, Task> callback)
        {
            await ReceiveActivityInternal(context, _middleware, callback).ConfigureAwait(false);
        }

        private static async Task ReceiveActivityInternal(
            IBotContext context, IEnumerable<IMiddleware> middleware, Func<IBotContext, Task> callback)
        {
            if (middleware == null)
                throw new ArgumentException(nameof(middleware));             

            if (middleware.Count() == 0) // No middleware to run.
            {
                // If all the Middlware ran, the "leading edge" of the tree is now complete. 
                // This means it's time to run any developer specified callback. 
                // Once this callback is done, the "trailing edge" calls are then completed. This
                // allows code that looks like:
                //      console.print("before");
                //      await next();
                //      console.print("after"); 
                // to run as expected. 

                if (callback != null)
                    await callback(context).ConfigureAwait(false);

                return;
            }

            // Default to "No more Middleware after this"
            async Task next()
            {
                // Remove the first item from the list of middleware to call,
                // so that the next call just has the remaining items to worry about. 
                IEnumerable<IMiddleware> remainingMiddleware = middleware.Skip(1);
                await ReceiveActivityInternal(context, remainingMiddleware, callback).ConfigureAwait(false);
            }

            // Grab the current middleware, which is the 1st element in the array, and execute it            
            await middleware.First().ReceiveActivity(context, next).ConfigureAwait(false);
        }

    }
}
