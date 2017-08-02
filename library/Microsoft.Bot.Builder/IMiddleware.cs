using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Builder
{
    public interface IContextInitializer
    {
        Task ContextCreated(BotContext context, CancellationToken token);
    }

    public interface IPostToBot
    {
        Task<bool> ReceiveActivity(BotContext context, CancellationToken token);
    }

    public interface IPostToUser
    {
        Task PostAsync(BotContext context, IList<IActivity> acitivties, CancellationToken token);
    }

    public interface IContextFinalizer
    {
        Task ContextDone(BotContext context, CancellationToken token);
    }

    public interface IMiddleware : IContextInitializer, IPostToBot, IPostToUser, IContextFinalizer
    {
    }
}
