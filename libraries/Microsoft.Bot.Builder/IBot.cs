// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Represents a bot that can operate on incoming activities.
    /// </summary>
    /// <remarks>A <see cref="BotAdapter"/> passes incoming activities from the user's
    /// channel to the bot's <see cref="OnTurnAsync(ITurnContext)"/> method.</remarks>
    /// <example>
    /// This defines a bot that responds with "Hello world!" to any incoming message.
    /// <code>
    /// public class EchoBot : IBot
    /// {
    ///     public async Task OnTurnAsync(ITurnContext context)
    ///     {
    ///         if (context.Activity.Type is ActivityTypes.Message)
    ///         {
    ///             await context.SendActivity("Hello world!");
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="IMiddleware"/>
    public interface IBot
    {
        /// <summary>
        /// When implemented in a bot, handles an incoming activity.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>The <paramref name="turnContext"/> provides information about the
        /// incoming activity, and other data needed to process the activity.</remarks>
        /// <seealso cref="ITurnContext"/>
        /// <seealso cref="Bot.Schema.IActivity"/>
        Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken));
    }
}
