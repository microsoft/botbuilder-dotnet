// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    ///  Middleware that will call `read()` and `write()` in parallel on multiple `BotState`
    ///  instances.
    /// </summary>
    public class BotStateSet : IMiddleware
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BotStateSet"/> class.
        /// </summary>
        /// <param name="botStates">initial list of <see cref="BotState"/> objects to manage.</param>
        public BotStateSet(params BotState[] botStates)
        {
            BotStates.AddRange(botStates);
        }

        /// <summary>
        /// Gets the list of state management objects managed by this object.
        /// </summary>
        /// <value>The state management objects managed by this object.</value>
        public List<BotState> BotStates { get; } = new List<BotState>();

        /// <summary>
        /// Add a BotState to the list of sources to load.
        /// </summary>
        /// <param name="botState">botState to manage.</param>
        /// <returns>botstateset for chaining more .Use().</returns>
        public BotStateSet Use(BotState botState)
        {
            BotStates.Add(botState);
            return this;
        }

        /// <summary>
        /// Middleware implementation which loads/savesChanges automatically.
        /// </summary>
        /// <param name="turnContext">turn context.</param>
        /// <param name="next">next middlware.</param>
        /// <param name="cancellationToken">cancellationToken.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            await next(cancellationToken).ConfigureAwait(false);
            await SaveChangesAsync(turnContext, false, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Load all BotState records in parallel.
        /// </summary>
        /// <param name="turnContext">turn context.</param>
        /// <param name="force">should data be forced into cache.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task LoadAsync(ITurnContext turnContext, bool force = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tasks = BotStates.Select(bs => bs.LoadAsync(turnContext, force, cancellationToken)).ToList();
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Save All BotState changes in parallel.
        /// </summary>
        /// <param name="context">turn context.</param>
        /// <param name="force">should data be forced to save even if no change were detected.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task SaveChangesAsync(ITurnContext turnContext, bool force = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tasks = BotStates.Select(bs => bs.SaveChangesAsync(turnContext, force, cancellationToken)).ToList();
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}
