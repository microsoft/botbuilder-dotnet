using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
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

        public async Task PostActivity(BotContext context, IList<Activity> activities)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.ActivityListNotNull(activities);

            await _bot.Adapter.Post(activities).ConfigureAwait(false);
        }

    }
}