// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Middleware
{
    public class MiddlewareSet : IMiddleware, IReceiveActivity, ISendActivity, IContextCreated
    {
        public delegate Task NextDelegate();
        private readonly IList<IMiddleware> _middleware = new List<IMiddleware>();

        public MiddlewareSet Use(IMiddleware middleware)
        {
            BotAssert.MiddlewareNotNull(middleware);
            _middleware.Add(middleware);
            return this;
        }

        public async Task ContextCreated(IBotContext context)
        {
            await ContextCreatedInternal(context, this._middleware.OfType<IContextCreated>().ToArray()).ConfigureAwait(false);
        }

        public async Task ContextCreated(IBotContext context, NextDelegate next)
        {
            await ContextCreatedInternal(context, this._middleware.OfType<IContextCreated>().ToArray()).ConfigureAwait(false);
        }

        private async Task ContextCreatedInternal(IBotContext context, IContextCreated[] middleware)
        {
            BotAssert.MiddlewareNotNull(middleware);

            if (middleware.Length == 0) // No middleware to run.
                return;

            async Task next()
            {
                // Remove the first item from the list of middleware to call,
                // so that the next call just has the remaining items to worry about. 
                IContextCreated[] remainingMiddleware = middleware.Skip(1).ToArray();
                await ContextCreatedInternal(context, remainingMiddleware).ConfigureAwait(false);
            }

            // Grab the current middleware, which is the 1st element in the array, and execute it            
            await middleware[0].ContextCreated(context, next).ConfigureAwait(false);
        }

        public async Task ReceiveActivity(IBotContext context)
        {
            await ReceiveActivityInternal(context, this._middleware.OfType<IReceiveActivity>().ToArray()).ConfigureAwait(false);
        }

        public async Task ReceiveActivity(IBotContext context, NextDelegate next)
        {
            await ReceiveActivityInternal(context, this._middleware.OfType<IReceiveActivity>().ToArray()).ConfigureAwait(false);
            await next().ConfigureAwait(false); 
        }

        /// <summary>
        /// Intended to be called from Bot, this method performs exactly the same as the
        /// standard ReceiveActivity, except that it returns TRUE if all Middlware in the receive
        /// pipeline was run, and FALSE if a middlware failed to run next.         
        /// </summary>
        /// <returns>True, if all executed middleware in the pipeline called Next(). 
        /// False, if one of the middleware instances did not call Next(). 
        /// </returns>
        public async Task<bool> ReceiveActivityWithStatus(IBotContext context)
        {
            return await ReceiveActivityInternal(context, this._middleware.OfType<IReceiveActivity>().ToArray()).ConfigureAwait(false);
        }        

        private async Task<bool> ReceiveActivityInternal(IBotContext context, IReceiveActivity[] middleware)
        {
            BotAssert.MiddlewareNotNull(middleware);
            bool didAllRun = false;

            if (middleware.Length == 0) // No middleware to run.
            {
                // If all the Middlware ran, let the caller know. 
                return true;
            }

            // Default to "No more Middleware after this"
            async Task next()
            {
                // Remove the first item from the list of middleware to call,
                // so that the next call just has the remaining items to worry about. 
                IReceiveActivity[] remainingMiddleware = middleware.Skip(1).ToArray();
                didAllRun |= await ReceiveActivityInternal(context, remainingMiddleware).ConfigureAwait(false);
            }

            // Grab the current middleware, which is the 1st element in the array, and execute it            
            await middleware[0].ReceiveActivity(context, next).ConfigureAwait(false);
            return didAllRun;
        }

        public async Task SendActivity(IBotContext context, IList<Activity> activities)
        {
            await SendActivityInternal(context, activities, this._middleware.OfType<ISendActivity>().ToArray()).ConfigureAwait(false);
        }

        public async Task SendActivity(IBotContext context, IList<Activity> activities, NextDelegate next)
        {
            await SendActivityInternal(context, activities, this._middleware.OfType<ISendActivity>().ToArray()).ConfigureAwait(false);
            await next().ConfigureAwait(false);
        }

        private async Task SendActivityInternal(IBotContext context, IList<Activity> activities, ISendActivity[] middleware)
        {
            BotAssert.MiddlewareNotNull(middleware);
            BotAssert.ActivityListNotNull(activities);

            if (middleware.Length == 0) // No middleware to run.
                return;

            async Task next()
            {
                // Remove the first item from the list of middleware to call,
                // so that the next call just has the remaining items to worry about. 
                ISendActivity[] remainingMiddleware = middleware.Skip(1).ToArray();
                await SendActivityInternal(context, activities, remainingMiddleware).ConfigureAwait(false);
            }

            // Grab the current middleware, which is the 1st element in the array, and execute it            
            await middleware[0].SendActivity(context, activities, next).ConfigureAwait(false);
        }
    }
}
