using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public class PostToConnectorMiddleware : IPostActivity, IContextDone, IContextCreated, IReceiveActivity
    {
        private readonly IConnector _connector;
        private readonly MiddlewareSet _inner;

        public PostToConnectorMiddleware(MiddlewareSet middlewareSet, IConnector connector)
        {
            _inner = middlewareSet ?? throw new ArgumentNullException("middlewareSet");
            _connector = connector ?? throw new ArgumentNullException("connector");
        }

        public MiddlewareSet MiddlewareSet => this._inner;

        public async Task ContextCreated(BotContext context, CancellationToken token)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.CancellationTokenNotNull(token);

            await this._inner.ContextCreated(context, token);
        }

        public async Task<ReceiveResponse> ReceiveActivity(BotContext context, CancellationToken token)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.CancellationTokenNotNull(token);

            return await this._inner.ReceiveActivity(context, token);
        }

        public async Task PostActivity(BotContext context, IList<IActivity> activities, CancellationToken token)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.ActivityListNotNull(activities);
            BotAssert.CancellationTokenNotNull(token);

            await this._inner.PostActivity(context, activities, token);
            foreach (var activity in activities)
            {
                context.Responses.Add(activity);
            }
            await this.FlushResponses(context, token);
        }

        public async Task ContextDone(BotContext context, CancellationToken token)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.CancellationTokenNotNull(token);

            await this._inner.ContextDone(context, token);
            await this.FlushResponses(context, token);
        }
        
        private async Task FlushResponses(BotContext context, CancellationToken token)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.CancellationTokenNotNull(token);

            await this._connector.Post(context.Responses, token);
            context.Responses.Clear();
        }
    }
}