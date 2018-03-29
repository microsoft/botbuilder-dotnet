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
    /// channel to the middleware's <see cref="OnProcessRequest(ITurnContext, NextDelegate)"/>
    /// method.
    /// <para>You can add middleware objects to your adapter’s middleware collection. The
    /// adapter processes and directs incoming activities in through the bot middleware 
    /// pipeline to your bot’s logic and then back out again. As each activity flows in 
    /// and out of the bot, each piece of middleware can inspect or act upon the activity, 
    /// both before and after the bot logic runs.</para>
    /// <para>For each activity, the adapter calls middleware in the order in which you 
    /// added it.</para>
    /// </remarks>
    /// <example>
    /// This defines middleware that sends "before" and "after" messages
    /// before and after the adapter calls the bot's 
    /// <see cref="IBot.OnReceiveActivity(ITurnContext)"/> method.
    /// <code>
    /// public class SampleMiddleware : IMiddleware
    /// {
    ///     public async Task OnProcessRequest(ITurnContext context, MiddlewareSet.NextDelegate next)
    ///     {
    ///         context.SendActivity("before");
    ///         await next().ConfigureAwait(false);
    ///         context.SendActivity("after");
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="IBot"/>
    public interface IMiddleware
    {
        /// <summary>
        /// Processess an incoming activity.
        /// </summary>
        /// <param name="context">The context object for this turn.</param>
        /// <param name="next">The delegate to call to continue the bot middleware pipeline.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>Middleware calls the <paramref name="next"/> delegate to pass control to 
        /// the next middleware in the pipeline. If middleware doesn’t call the next delegate,
        /// the adapter does not call any of the subsequent middleware’s request handlers or the 
        /// bot’s receive handler, and the pipeline short circuits.
        /// <para>The <paramref name="context"/> provides information about the 
        /// incoming activity, and other data needed to process the activity.</para>
        /// </remarks>
        /// <seealso cref="ITurnContext"/>
        /// <seealso cref="Bot.Schema.IActivity"/>
        Task OnProcessRequest(ITurnContext context, MiddlewareSet.NextDelegate next);
    }

    /// <summary>
    /// Helper class for defining middleware by using a delegate or anonymous method.
    /// </summary>
    public class AnonymousReceiveMiddleware : IMiddleware
    {
        private readonly Func<ITurnContext, NextDelegate, Task> _toCall;

        /// <summary>
        /// Creates a middleware object that uses the provided method as its
        /// process request handler.
        /// </summary>
        /// <param name="anonymousMethod">The method to use as the middleware's process 
        /// request handler.</param>
        public AnonymousReceiveMiddleware(Func<ITurnContext, NextDelegate, Task> anonymousMethod)
        {
            _toCall = anonymousMethod ?? throw new ArgumentNullException(nameof(anonymousMethod));
        }

        /// <summary>
        /// Uses the method provided in the <see cref="AnonymousReceiveMiddleware"/> to
        /// process an incoming activity.
        /// </summary>
        /// <param name="context">The context object for this turn.</param>
        /// <param name="next">The delegate to call to continue the bot middleware pipeline.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public Task OnProcessRequest(ITurnContext context, NextDelegate next)
        {
            return _toCall(context, next);
        }
    }   
}
