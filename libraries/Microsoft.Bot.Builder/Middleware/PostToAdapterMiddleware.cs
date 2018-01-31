// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Middleware
{
    public class PostToAdapterMiddleware : IPostActivity
    {
        private readonly Bot _bot;

        public PostToAdapterMiddleware(Bot b)
        {
            _bot = b ?? throw new ArgumentNullException(nameof(Bot));
        }        

        public async Task PostActivity(IBotContext context, IList<IActivity> activities, Middleware.MiddlewareSet.NextDelegate next)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.ActivityListNotNull(activities);

            await next().ConfigureAwait(false); 
            await _bot.Adapter.Post(activities).ConfigureAwait(false);            
        }
    }
}