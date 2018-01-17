using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Builder.Middleware
{
    public class MiddlewareSet : IMiddleware, IReceiveActivity, IPostActivity, IContextCreated
    {
        public delegate Task NextDelegate();

        private IList<IMiddleware> _middleware = new List<IMiddleware>();

        public void Clear()
        {
            _middleware.Clear();
        }

        internal IList<IMiddleware> Middleware {  get { return _middleware; } }

        public MiddlewareSet Use(IMiddleware middleware)
        {
            BotAssert.MiddlewareNotNull(middleware);
            _middleware.Add(middleware);
            return this;
        }

        public MiddlewareSet OnReceive(Func<IBotContext, NextDelegate, Task> anonymousMethod)
        {
            if (anonymousMethod == null)
                throw new ArgumentNullException(nameof(anonymousMethod));

            return this.Use(new AnonymousReceiveMiddleware(anonymousMethod));
        }

        public MiddlewareSet OnContextCreated(Func<IBotContext, NextDelegate, Task> anonymousMethod)
        {
            if (anonymousMethod == null)
                throw new ArgumentNullException(nameof(anonymousMethod));

            return this.Use(new AnonymousContextCreatedMiddleware(anonymousMethod));
        }

        public MiddlewareSet OnPostActivity(Func<IBotContext, IList<IActivity>, NextDelegate, Task> anonymousMethod)
        {
            if (anonymousMethod == null)
                throw new ArgumentNullException(nameof(anonymousMethod));

            return this.Use(new AnonymousPostActivityMiddleware(anonymousMethod));
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
            
            NextDelegate next = async () => {
                // Remove the first item from the list of middleware to call,
                // so that the next call just has the remaining items to worry about. 
                IContextCreated[] remainingMiddleware = middleware.Skip(1).ToArray();
                await ContextCreatedInternal(context, remainingMiddleware).ConfigureAwait(false);
            };

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

        private async Task ReceiveActivityInternal(IBotContext context, IReceiveActivity[] middleware)
        {
            BotAssert.MiddlewareNotNull(middleware);

            if (middleware.Length == 0) // No middleware to run.
                return;

            // Default to "No more Middleware after this"
            NextDelegate next = async () => 
            {
                // Remove the first item from the list of middleware to call,
                // so that the next call just has the remaining items to worry about. 
                IReceiveActivity[] remainingMiddleware = middleware.Skip(1).ToArray();
                await ReceiveActivityInternal(context, remainingMiddleware).ConfigureAwait(false);
            };

            // Grab the current middleware, which is the 1st element in the array, and execute it            
            await middleware[0].ReceiveActivity(context, next).ConfigureAwait(false);
        }



        public async Task PostActivity(IBotContext context, IList<IActivity> activities)
        {
            await PostActivityInternal(context, activities, this._middleware.OfType<IPostActivity>().ToArray()).ConfigureAwait(false);
        }

        public async Task PostActivity(IBotContext context, IList<IActivity> activities, NextDelegate next)
        {
            await PostActivityInternal(context, activities, this._middleware.OfType<IPostActivity>().ToArray()).ConfigureAwait(false);
            await next().ConfigureAwait(false);
        }

        private async Task PostActivityInternal(IBotContext context, IList<IActivity> activities, IPostActivity[] middleware)
        {
            BotAssert.MiddlewareNotNull(middleware);
            BotAssert.ActivityListNotNull(activities); 

            if (middleware.Length == 0) // No middleware to run.
                return;

            NextDelegate next = async () => 
            {
                // Remove the first item from the list of middleware to call,
                // so that the next call just has the remaining items to worry about. 
                IPostActivity[] remainingMiddleware = middleware.Skip(1).ToArray();
                await PostActivityInternal(context, activities, remainingMiddleware).ConfigureAwait(false);
            };

            // Grab the current middleware, which is the 1st element in the array, and execute it            
            await middleware[0].PostActivity(context, activities, next).ConfigureAwait(false);
        }       
    }
}
