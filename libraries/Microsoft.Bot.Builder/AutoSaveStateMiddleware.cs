// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    ///  Middleware to automatically persist state before the end of each turn.
    /// </summary>
    /// <remarks>
    /// This calls <see cref="BotState.SaveChangesAsync(ITurnContext, bool, CancellationToken)"/>
    /// on each state object it manages.
    /// </remarks>
    public class AutoSaveStateMiddleware : IMiddleware
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutoSaveStateMiddleware"/> class.
        /// </summary>
        /// <param name="botStates">initial list of <see cref="BotState"/> objects to manage.</param>
        public AutoSaveStateMiddleware(params BotState[] botStates)
        {
            BotStateSet = new BotStateSet(botStates);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoSaveStateMiddleware"/> class with 
        /// a list of state management objects managed by this object.
        /// </summary>
        /// <param name="botStateSet">The state management objects managed by this object.</param>
        public AutoSaveStateMiddleware(BotStateSet botStateSet)
        {
            BotStateSet = botStateSet;
        }

        /// <summary>
        /// Gets or sets the list of state management objects managed by this object.
        /// </summary>
        /// <value>The state management objects managed by this object.</value>
        public BotStateSet BotStateSet { get; set; }

        /// <summary>
        /// Adds a state management object to the list of states to manage.
        /// </summary>
        /// <param name="botState">The bot state to add.</param>
        /// <returns>The updated <see cref="BotStateSet"/> object.</returns>
        public AutoSaveStateMiddleware Add(BotState botState)
        {
            if (botState == null)
            {
                throw new ArgumentNullException(nameof(botState));
            }

            this.BotStateSet.Add(botState);
            return this;
        }

        /// <summary>
        /// Before the turn ends, calls <see cref="BotState.SaveChangesAsync(ITurnContext, bool, CancellationToken)"/>
        /// on each state object.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="next">The delegate to call to continue the bot middleware pipeline.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>This middleware persists state after the bot logic completes and before the turn ends.</remarks>
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            await next(cancellationToken).ConfigureAwait(false);
            await this.BotStateSet.SaveAllChangesAsync(turnContext, false, cancellationToken).ConfigureAwait(false);
        }
    }
}
