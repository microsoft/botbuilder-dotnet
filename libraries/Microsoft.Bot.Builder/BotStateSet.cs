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
    /// <remarks>
    ///     This example shows boilerplate code for reading and writing conversation and user state within
    ///      a bot:
    ///
    ///      ```JavaScript
    ///      const { BotStateSet, ConversationState, UserState, MemoryStorage } = require('botbuilder');
    ///
    ///      const storage = new MemoryStorage();
    ///      const conversationState = new ConversationState(storage);
    ///      const userState = new UserState(storage);
    ///
    ///      adapter.use(new BotStateSet(conversationState, userState));
    ///
    ///      server.post('/api/messages', (req, res) => {
    ///      adapter.processActivity(req, res, async (context) => {
    ///            // ... route activity ...
    ///
    ///  ```
    /// </remarks>
    public class BotStateSet : IMiddleware
    {
        private List<BotState> botStates = new List<BotState>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BotStateSet"/> class.
        /// </summary>
        /// <param name="botStates">initial list of BotState to manage</param>
        public BotStateSet(params BotState[] botStates)
        {
            this.botStates.AddRange(botStates);
        }

        /// <summary>
        /// Add a BotState to the list of sources to load.
        /// </summary>
        /// <param name="botState">botState to manage</param>
        /// <returns>botstateset for chaining more .Use()</returns>
        public BotStateSet Use(BotState botState)
        {
            this.botStates.Add(botState);
            return this;
        }

        /// <summary>
        /// Middleware implementation which loads/savesChanges automatically
        /// </summary>
        /// <param name="context">turn context.</param>
        /// <param name="next">next middlware.</param>
        /// <param name="cancellationToken">cancellationToken.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task OnTurnAsync(ITurnContext context, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            await this.LoadAsync(context, true).ConfigureAwait(false);
            await next(cancellationToken).ConfigureAwait(false);
            await this.SaveChangesAsync(context).ConfigureAwait(false);
        }

        /// <summary>
        /// Load all BotState records in parallel.
        /// </summary>
        /// <param name="context">turn context</param>
        /// <param name="force">should data be forced into cache.</param>
        /// <param name="cancellationToken">Cancelation token.</param>
        /// <returns>task</returns>
        public async Task LoadAsync(ITurnContext context, bool force = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tasks = this.botStates.Select(bs => bs.LoadAsync(context, force, cancellationToken)).ToList();
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Save All BotState changes in parallel.
        /// </summary>
        /// <param name="context">turn context.</param>
        /// <param name="force">should data be forced to save even if no change were detected.</param>
        /// <returns>task</returns>
        public async Task SaveChangesAsync(ITurnContext context, bool force = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tasks = this.botStates.Select(bs => bs.SaveChangesAsync(context, force, cancellationToken)).ToList();
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}
