using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public class PostToConnectorMiddleware : IPostActivity
    {
        private readonly Bot _bot;

        public PostToConnectorMiddleware(Bot b)
        {
            _bot = b ?? throw new ArgumentNullException(nameof(Bot)); 
        }
                
        public async Task PostActivity(BotContext context, IList<Activity> activities, CancellationToken token)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.ActivityListNotNull(activities);
            BotAssert.CancellationTokenNotNull(token);

            await _bot.Connector.Post(context.Responses, token).ConfigureAwait(false);
        }        
    }
}