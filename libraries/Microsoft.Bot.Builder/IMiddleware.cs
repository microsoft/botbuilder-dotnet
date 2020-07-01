// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Encapsulates an asynchronous method that calls the next
    /// <see cref="IMiddleware"/>.<see cref="IMiddleware.OnTurnAsync"/>
    /// or <see cref="IBot"/>.<see cref="IBot.OnTurnAsync"/> method in the middleware pipeline.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects
    /// or threads to receive notice of cancellation.</param>
    /// <returns>A task that represents the work queued to execute.</returns>
    public delegate Task NextDelegate(CancellationToken cancellationToken);

    /// <summary>
    /// Represents middleware that can operate on incoming activities.
    /// </summary>
    /// <remarks>A <see cref="BotAdapter"/> passes incoming activities from the user's
    /// channel to the middleware's <see cref="OnTurnAsync(ITurnContext, NextDelegate, CancellationToken)"/>
    /// method.
    /// <para>You can add middleware objects to your adapter’s middleware collection. The
    /// adapter processes and directs incoming activities in through the bot middleware
    /// pipeline to your bot’s logic and then back out again. As each activity flows in
    /// and out of the bot, each piece of middleware can inspect or act upon the activity,
    /// both before and after the bot logic runs.</para>
    /// <para>For each activity, the adapter calls middleware in the order in which you
    /// added it.</para>
    /// </remarks>
    /// <seealso cref="IBot"/>
    public interface IMiddleware
    {
        /// <summary>
        /// When implemented in middleware, processes an incoming activity.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="next">The delegate to call to continue the bot middleware pipeline.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>Middleware calls the <paramref name="next"/> delegate to pass control to
        /// the next middleware in the pipeline. If middleware doesn’t call the next delegate,
        /// the adapter does not call any of the subsequent middleware’s request handlers or the
        /// bot’s receive handler, and the pipeline short circuits.
        /// <para>The <paramref name="turnContext"/> provides information about the
        /// incoming activity, and other data needed to process the activity.</para>
        /// </remarks>
        /// <seealso cref="ITurnContext"/>
        /// <seealso cref="Bot.Schema.IActivity"/>
#pragma warning disable CA1716 // Identifiers should not match keywords (we can't change this without breaking binary compat)
        Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken));
#pragma warning restore CA1716 // Identifiers should not match keywords
    }
}
