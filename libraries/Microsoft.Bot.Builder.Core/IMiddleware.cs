// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using static Microsoft.Bot.Builder.MiddlewareSet;

namespace Microsoft.Bot.Builder
{
    public interface IMiddleware
    {
        Task ReceiveActivity(IBotContext context, MiddlewareSet.NextDelegate next);
    }

    public class AnonymousReceiveMiddleware : IMiddleware
    {
        private readonly Func<IBotContext, NextDelegate, Task> _toCall;

        public AnonymousReceiveMiddleware(Func<IBotContext, NextDelegate, Task> anonymousMethod)
        {
            _toCall = anonymousMethod ?? throw new ArgumentNullException(nameof(anonymousMethod));
        }

        public Task ReceiveActivity(IBotContext context, NextDelegate next)
        {
            return _toCall(context, next);
        }
    }   
}
