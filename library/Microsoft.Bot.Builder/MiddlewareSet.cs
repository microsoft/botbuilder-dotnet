using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public class MiddlewareSet : IContextCreated, IPostActivity, IReceiveActivity, IContextDone
    {
        private readonly IList<IMiddleware> _middlewareList;

        public MiddlewareSet()
        {
            _middlewareList = new List<IMiddleware>();
        }

        public MiddlewareSet(IList<IMiddleware> items)
        {
            _middlewareList = items ?? throw new ArgumentNullException(nameof(items));
        }

        public IList<IMiddleware> Middlewares => _middlewareList;

        public void RemoveAll()
        {
            _middlewareList.Clear();
        }

        public IList<IMiddleware> Middleware
        {
            get { return _middlewareList; }
        }

        public MiddlewareSet Use(params IMiddleware[] middlewareItems)
        {
            if (middlewareItems == null)
                throw new ArgumentNullException(nameof(middlewareItems));

            foreach (var m in middlewareItems)
                _middlewareList.Add(m);

            return this;
        }

        public virtual async Task ContextCreated(BotContext context)
        {
            BotAssert.ContextNotNull(context);

            context.Bot.Logger.Information($"Middleware: Context Created for {context.ConversationReference.ActivityId}.");

            foreach (var m in this._middlewareList.Where<IContextCreated>())
                await m.ContextCreated(context).ConfigureAwait(false);
        }

        public virtual async Task<ReceiveResponse> ReceiveActivity(BotContext context)
        {
            BotAssert.ContextNotNull(context);

            ReceiveResponse response = new ReceiveResponse(false);
            if (context.Request != null)
            {
                context.Bot.Logger.Information($"Middleware: ReceiveActivity for {context.Request.Id}.");

                foreach (var middleware in this._middlewareList.Where<IReceiveActivity>())
                {
                    response = await middleware.ReceiveActivity(context).ConfigureAwait(false);
                    if (response?.Handled == true) break;
                }
            }
            return response;
        }

        public virtual async Task ContextDone(BotContext context)
        {
            BotAssert.ContextNotNull(context);

            context.Bot.Logger.Information($"Middleware: ContextDone for {context.ConversationReference.ActivityId}.");
            foreach (var m in this._middlewareList.Where<IContextDone>().Reverse())
                await m.ContextDone(context).ConfigureAwait(false);
        }

        public virtual async Task PostActivity(BotContext context, IList<Activity> activities)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.ActivityListNotNull(activities);
            if (activities != null && activities.Any())
            {
                context.Bot.Logger.Information($"Middleware: PostActivity for {context.ConversationReference.ActivityId}.");
                foreach (var m in this._middlewareList.Where<IPostActivity>().Reverse())
                    await m.PostActivity(context, activities).ConfigureAwait(false);
            }
        }

        public virtual async Task RunPipeline(BotContext context, Func<BotContext, Task> proactiveCallback = null)
        {
            BotAssert.ContextNotNull(context);

            context.Bot.Logger.Information($"Middleware: Beginning Pipeline for {context.ConversationReference.ActivityId}");

            // Call any registered Middleware Components looking for ContextCreated()
            await this.ContextCreated(context).ConfigureAwait(false);

            // Call any registered Middleware Components looking for ReceiveActivity()
            if (context.Request != null)
                await this.ReceiveActivity(context).ConfigureAwait(false);

            // call back to caller
            if (proactiveCallback != null)
                await proactiveCallback(context).ConfigureAwait(false);

            // Call any registered Middleware Components looking for PostActivity()
            if (context.Responses != null && context.Responses.Any())
                await this.PostActivity(context, context.Responses).ConfigureAwait(false);

            // Call any registered Middleware Components looking for ContextDone()
            await this.ContextDone(context).ConfigureAwait(false);

            context.Bot.Logger.Information($"Middleware: Ending Pipeline for {context.ConversationReference.ActivityId}");
        }
    }
}
