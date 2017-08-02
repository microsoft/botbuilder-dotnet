using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Builder
{
    public class MiddlewareSet : IMiddleware
    {
        private readonly IList<IMiddleware> middlewares;
        public MiddlewareSet(IList<IMiddleware> middlewares)
        {
            SetField.NotNull(out this.middlewares, nameof(middlewares), middlewares);
        }

        public IList<IMiddleware> Middlewares => middlewares;

        public virtual async Task ContextCreated(BotContext context, CancellationToken token)
        {
            foreach (var m in middlewares)
                await m.ContextCreated(context, token);
        }

        public virtual async Task<bool> ReceiveActivity(BotContext context, CancellationToken token)
        {
            var handled = false;
            foreach (var middleware in middlewares)
            {
                handled = await middleware.ReceiveActivity(context, token);
                if (handled) break;
            }
            return handled;
        }

        public virtual async Task ContextDone(BotContext context, CancellationToken token)
        {
            foreach (var m in middlewares.Reverse())
                await m.ContextDone(context, token);
        }

        public virtual async Task PostAsync(BotContext context, IList<IActivity> acitivties, CancellationToken token)
        {
            foreach (var m in middlewares.Reverse())
                await m.PostAsync(context, acitivties, token);
        }
    }
}
