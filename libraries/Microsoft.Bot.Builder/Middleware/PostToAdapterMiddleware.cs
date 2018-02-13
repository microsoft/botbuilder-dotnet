// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Middleware
{
    public class SendToAdapterMiddleware : ISendActivity
    {
        private readonly Bot _bot;

        public SendToAdapterMiddleware(Bot b)
        {
            _bot = b ?? throw new ArgumentNullException(nameof(Bot));
        }        

        public async Task SendActivity(IBotContext context, IList<IActivity> activities, Middleware.MiddlewareSet.NextDelegate next)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.ActivityListNotNull(activities);

            await next().ConfigureAwait(false); 
            await _bot.Adapter.Send(activities).ConfigureAwait(false);            
        }
    }
}