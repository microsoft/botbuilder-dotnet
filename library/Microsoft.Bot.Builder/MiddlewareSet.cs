using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
            _middlewareList = items ?? throw new ArgumentNullException("items");
        }

        public IList<IMiddleware> Middlewares => _middlewareList;

        public void RemoveAll()
        {
            _middlewareList.Clear(); 
        }

        public IList<IMiddleware> Middleware
        { 
            get { return _middlewareList;  }
        }
       
        public MiddlewareSet Use(params IMiddleware[] middlewareItems)
        {
            if (middlewareItems == null)
                throw new ArgumentNullException("middlewareItems");

            foreach (var m in middlewareItems)
                _middlewareList.Add(m);

            return this;
        }

        public virtual async Task ContextCreated(BotContext context, CancellationToken token)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.CancellationTokenNotNull(token);

            context.Bot.Logger.Information($"Middleware: Context Created for {context.Request.Id}.");

            foreach (var m in this._middlewareList.Where<IContextCreated>())
                await m.ContextCreated(context, token).ConfigureAwait(false);
        }

        public virtual async Task<ReceiveResponse> ReceiveActivity(BotContext context, CancellationToken token)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.CancellationTokenNotNull(token);

            context.Bot.Logger.Information($"Middleware: ReceiveActivity for {context.Request.Id}.");

            ReceiveResponse response = new ReceiveResponse(false);
            foreach (var middleware in this._middlewareList.Where<IReceiveActivity>())
            {
                response = await middleware.ReceiveActivity(context, token).ConfigureAwait(false);
                if (response?.Handled == true) break;
            }
            return response;
        }

        public virtual async Task ContextDone(BotContext context, CancellationToken token)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.CancellationTokenNotNull(token);

            context.Bot.Logger.Information($"Middleware: ContextDone for {context.Request.Id}.");
            foreach (var m in this._middlewareList.Where<IContextDone>().Reverse())
                await m.ContextDone(context, token).ConfigureAwait(false);
        }

        public virtual async Task PostActivity(BotContext context, IList<Activity> activities, CancellationToken token)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.ActivityListNotNull(activities);
            BotAssert.CancellationTokenNotNull(token);

            context.Bot.Logger.Information($"Middleware: PostActivity for {context.Request.Id}.");
            foreach (var m in this._middlewareList.Where<IPostActivity>().Reverse())
                await m.PostActivity(context, activities, token).ConfigureAwait(false);
        }

        public virtual async Task<bool> RunPipeline(BotContext context, CancellationToken token)
        {
            BotAssert.ContextNotNull(context);            
            BotAssert.CancellationTokenNotNull(token);

            context.Bot.Logger.Information($"Middleware: Beginning Pipeline for {context.Request.Id}");

            //Call any registered Middleware components looking for context creation events.
            await this.ContextCreated(context, token).ConfigureAwait(false);

            //Call any registered Middleware Components looking for Received Activities
            await this.ReceiveActivity(context, token).ConfigureAwait(false);

            //Call any registered Middleware Components looking for handle posting of activities
            await this.PostActivity(context, context.Responses, token).ConfigureAwait(false);

            //Call any registered Middleware Components looking for handle posting of activities
            await this.ContextDone(context, token).ConfigureAwait(false);

            context.Bot.Logger.Information($"Middleware: Ending Pipeline for {context.Request.Id}");

            return true;
        }
    }
}
