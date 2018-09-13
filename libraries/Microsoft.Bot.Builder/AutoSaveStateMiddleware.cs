// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    ///  Middleware to automatically call .SaveChanges() at the end of the turn for all BotState class it is managing.
    /// </summary>
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
        /// Add a BotState to the list of sources to load.
        /// </summary>
        /// <param name="botState">botState to manage.</param>
        /// <returns>botstateset for chaining more .Use().</returns>
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
        /// Middleware implementation which calls savesChanges automatically at the end of the turn.
        /// </summary>
        /// <param name="turnContext">turn context.</param>
        /// <param name="next">next middlware.</param>
        /// <param name="cancellationToken">cancellationToken.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            await next(cancellationToken).ConfigureAwait(false);
            await this.BotStateSet.SaveAllChangesAsync(turnContext, false, cancellationToken).ConfigureAwait(false);
        }
    }
}
