using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Builder
{
    public interface IMiddleware
    {
    }

    public interface IContextInitializer : IMiddleware
    {
        Task ContextCreated(BotContext context, CancellationToken token);
    }

    public interface IPostToBot : IMiddleware
    {
        Task<bool> ReceiveActivity(BotContext context, CancellationToken token);
    }

    public interface IPostToUser : IMiddleware
    {
        Task Post(BotContext context, IList<IActivity> acitivties, CancellationToken token);
    }

    public interface IContextFinalizer : IMiddleware
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
