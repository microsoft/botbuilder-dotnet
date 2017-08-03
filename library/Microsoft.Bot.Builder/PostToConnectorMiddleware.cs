using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Builder
{
    public class PostToConnectorMiddleware : IPostToUser, IContextFinalizer, IContextInitializer, IPostToBot
    {
        private readonly IConnector connector;
        private readonly MiddlewareSet inner;

        public PostToConnectorMiddleware(MiddlewareSet middlewareSet, IConnector connector)
        {
            SetField.NotNull(out this.connector, nameof(connector), connector);
            SetField.NotNull(out this.inner, nameof(inner), middlewareSet);
        }

        public MiddlewareSet MiddlewareSet => this.inner;

        public async Task ContextCreated(BotContext context, CancellationToken token)
        {
            await this.inner.ContextCreated(context, token);
        }

        public async Task<bool> ReceiveActivity(BotContext context, CancellationToken token)
        {
            return await this.inner.ReceiveActivity(context, token);
        }

        public async Task Post(BotContext context, IList<IActivity> activities, CancellationToken token)
        {
            await this.inner.Post(context, activities, token);
            foreach (var activity in activities)
            {
                context.Responses.Add(activity);
            }
            await this.FlushResponses(context, token);
        }

        public async Task ContextDone(BotContext context, CancellationToken token)
        {
            await this.inner.ContextDone(context, token);
            await this.FlushResponses(context, token);
        }
        
        private async Task FlushResponses(BotContext context, CancellationToken token)
        {
            await this.connector.Post(context.Responses, token);
            context.Responses.Clear();
        }
    }
}
