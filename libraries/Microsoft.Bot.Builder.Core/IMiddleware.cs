// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using static Microsoft.Bot.Builder.MiddlewareSet;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Represents middleware that can operate on incoming activities.
    /// </summary>
    /// <remarks>A <see cref="BotAdapter"/> passes incoming activities from the user's 
    /// channel to the bot's <see cref="IBot.OnReceiveActivity(ITurnContext)"/>.
    /// <para>Middleware is added to the adapter at initialization time, and can 
    /// establish or persist state, react to incoming requests, or short circuit the 
    /// pipeline. The SDK provides some predefined middleware, but you can also define 
    /// your own.</para></remarks>
    /// <seealso cref="Bot.Schema.IActivity"/>
    /// <seealso cref="ITurnContext"/>
    public interface IMiddleware
    {
        Task OnProcessRequest(ITurnContext context, MiddlewareSet.NextDelegate next);
    }

    public class AnonymousReceiveMiddleware : IMiddleware
    {
        private readonly Func<ITurnContext, NextDelegate, Task> _toCall;

        public AnonymousReceiveMiddleware(Func<ITurnContext, NextDelegate, Task> anonymousMethod)
        {
            _toCall = anonymousMethod ?? throw new ArgumentNullException(nameof(anonymousMethod));
        }

        public Task OnProcessRequest(ITurnContext context, NextDelegate next)
        {
            return _toCall(context, next);
        }
    }   
}
