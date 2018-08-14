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
        /// <param name="botStates">initial list of BotState to manage.</param>
        public BotStateSet(params BotState[] botStates)
        {
            BotStates.AddRange(botStates);
        }

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
        /// <param name="context">turn context.</param>
        /// <param name="next">next middlware.</param>
        /// <param name="cancellationToken">cancellationToken.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task OnTurnAsync(ITurnContext context, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            await next(cancellationToken).ConfigureAwait(false);
            await SaveChangesAsync(context, false, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Load all BotState records in parallel.
        /// </summary>
        /// <param name="context">turn context.</param>
        /// <param name="force">should data be forced into cache.</param>
        /// <param name="cancellationToken">Cancelation token.</param>
        /// <returns>task</returns>
        public async Task LoadAsync(ITurnContext context, bool force = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tasks = BotStates.Select(bs => bs.LoadAsync(context, force, cancellationToken)).ToList();
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Save All BotState changes in parallel.
        /// </summary>
        /// <param name="context">turn context.</param>
        /// <param name="force">should data be forced to save even if no change were detected.</param>
        /// <param name="cancellationToken">Cancelation token.</param>
        /// <returns>task</returns>
        public async Task SaveChangesAsync(ITurnContext context, bool force = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tasks = BotStates.Select(bs => bs.SaveChangesAsync(context, force, cancellationToken)).ToList();
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}
