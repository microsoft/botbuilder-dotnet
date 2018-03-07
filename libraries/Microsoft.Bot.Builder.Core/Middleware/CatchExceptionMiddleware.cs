// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Core.Middleware
{
    public class CatchExceptionMiddleware : IReceiveActivity, IContextCreated, ISendActivity
    {
        private readonly CatchExceptionHandler _handler;

        public CatchExceptionMiddleware(CatchExceptionHandler handler)
        {
            _handler = handler;
        }

        public async Task ReceiveActivity(IBotContext context, MiddlewareSet.NextDelegate next)
        {
            await CatchError(context, "receiveActivity", next);
        }

        public async Task ContextCreated(IBotContext context, MiddlewareSet.NextDelegate next)
        {
            await CatchError(context, "contextCreated", next);
        }

        public async Task SendActivity(IBotContext context, IList<Activity> activities, MiddlewareSet.NextDelegate next)
        {
            await CatchError(context, "sendActivity", next);
        }

        private async Task CatchError(IBotContext context, string phase, MiddlewareSet.NextDelegate next)
        {
            try
            {
                await next().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await _handler.Invoke(context, phase, ex);
            }
        }

        public delegate Task CatchExceptionHandler(IBotContext context, string phase, Exception exception);
    }
}
