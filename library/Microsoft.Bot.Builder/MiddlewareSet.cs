using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Builder
{
    public class MiddlewareSet : IContextCreated, IPostActivity, IReceiveActivity, IContextDone
    {
        private readonly IList<IMiddleware> middlewares;
        public MiddlewareSet(IList<IMiddleware> middlewares)
        {
            SetField.NotNull(out this.middlewares, nameof(middlewares), middlewares);
        }

        public IList<IMiddleware> Middlewares => middlewares;

        public virtual async Task ContextCreated(BotContext context, CancellationToken token)
        {
            foreach (var m in this.middlewares.Where<IContextCreated>())
                await m.ContextCreated(context, token);
        }

        public virtual async Task<ReceiveResponse> ReceiveActivity(BotContext context, CancellationToken token)
        {
            ReceiveResponse response=null;
            foreach (var middleware in this.middlewares.Where<IReceiveActivity>())
            {
                response = await middleware.ReceiveActivity(context, token);
                if (response?.Handled == true) break;
            }
            return response;
        }

        public virtual async Task ContextDone(BotContext context, CancellationToken token)
        {
            foreach (var m in this.middlewares.Where<IContextDone>().Reverse())
                await m.ContextDone(context, token);
        }

        public virtual async Task PostActivity(BotContext context, IList<IActivity> acitivties, CancellationToken token)
        {
            foreach (var m in this.middlewares.Where<IPostActivity>().Reverse())
                await m.PostActivity(context, acitivties, token);
        }
    }
}
