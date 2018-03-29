// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Microsoft.Bot.Builder;
using System.Threading.Tasks;

namespace Microsoft.Bot
{
    /// <summary>
    /// Represents a bot that can operate on incoming activities.
    /// </summary>
    /// <remarks>A <see cref="BotAdapter"/> passes incoming activities from the user's 
    /// channel to the bot's <see cref="OnReceiveActivity(ITurnContext)"/>.</remarks>
    /// <example>
    /// This defines a bot that responds with "Hello world!" to any incoming message.
    /// <code>
    /// public class EchoBot : IBot
    /// {
    ///     public async Task OnReceiveActivity(ITurnContext context)
    ///     {
    ///         if (context.Activity.Type is ActivityTypes.Message)
    ///         {
    ///             await context.SendActivity("Hello world!");
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="Bot.Schema.IActivity"/>
    /// <seealso cref="ITurnContext"/>
    public interface IBot
    {
        /// <summary>
        /// Handles an incoming activity.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        Task OnReceiveActivity(ITurnContext turnContext);
    }
}