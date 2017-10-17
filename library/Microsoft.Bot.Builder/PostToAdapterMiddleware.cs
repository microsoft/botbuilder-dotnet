using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Adapters;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public class PostToAdapterMiddleware : IPostActivity
    {
        private readonly Bot _bot;

        public PostToAdapterMiddleware(Bot b)
        {
            _bot = b ?? throw new ArgumentNullException(nameof(Bot)); 
        }
                
        public async Task PostActivity(BotContext context, IList<Activity> activities, CancellationToken token)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.ActivityListNotNull(activities);
            BotAssert.CancellationTokenNotNull(token);

            await _bot.Adapter.Post(context.Responses, token).ConfigureAwait(false);
        }        
    }
}