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

        public MiddlewareSet(IList<IMiddleware> items)
        {
            _middlewareList = items ?? throw new ArgumentNullException("items");
        }

        public IList<IMiddleware> Middlewares => _middlewareList;

        public void Add(IMiddleware middleware)
        {
            if (middleware == null)
                throw new ArgumentNullException("middleware");

            _middlewareList.Add(middleware);
        }

        public void AddRange(params IMiddleware[] middlewareItems)
        {
            if (middlewareItems == null)
                throw new ArgumentNullException("middlewareItems");

            foreach (var m in middlewareItems)            
                this.Add(m);            
        }

        public virtual async Task ContextCreated(BotContext context, CancellationToken token)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.CancellationTokenNotNull(token);

            foreach (var m in this._middlewareList.Where<IContextCreated>())
                await m.ContextCreated(context, token);
        }

        public virtual async Task<ReceiveResponse> ReceiveActivity(BotContext context, CancellationToken token)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.CancellationTokenNotNull(token);

            ReceiveResponse response = null;
            foreach (var middleware in this._middlewareList.Where<IReceiveActivity>())
            {
                response = await middleware.ReceiveActivity(context, token);
                if (response?.Handled == true) break;
            }
            return response;
        }

        public virtual async Task ContextDone(BotContext context, CancellationToken token)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.CancellationTokenNotNull(token);

            foreach (var m in this._middlewareList.Where<IContextDone>().Reverse())
                await m.ContextDone(context, token);
        }

        public virtual async Task PostActivity(BotContext context, IList<IActivity> activities, CancellationToken token)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.ActivityListNotNull(activities);
            BotAssert.CancellationTokenNotNull(token);

            foreach (var m in this._middlewareList.Where<IPostActivity>().Reverse())
                await m.PostActivity(context, activities, token);
        }
    }
}
