using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public interface IMiddleware { }

    public interface IContextCreated : IMiddleware
    {
        Task ContextCreated(BotContext context, CancellationToken token);
    }

    public interface IReceiveActivity : IMiddleware
    {
        Task<ReceiveResponse> ReceiveActivity(BotContext context, CancellationToken token);
    }

    public interface IPostActivity : IMiddleware
    {
        Task PostActivity(BotContext context, IList<IActivity> activities, CancellationToken token);
    }

    public interface IContextDone : IMiddleware
    {
        Task ContextDone(BotContext context, CancellationToken token);
    }

    public static partial class MiddlewareExtensions
    {
        public static IEnumerable<T> Where<T>(this IList<IMiddleware> middlewares) where T : IMiddleware
        {
            return middlewares.Where(x => x is T).Cast<T>();
        }
    }
}
