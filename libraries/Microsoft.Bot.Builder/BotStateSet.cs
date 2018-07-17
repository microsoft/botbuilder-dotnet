using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
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
    ///     
    ///  ```
    /// </remarks>
    public class BotStateSet : IMiddleware
    {
        private List<BotState> botStates = new List<BotState>();

        public BotStateSet(params BotState[] botstates)
        {
            this.botStates.AddRange(botStates);
        }

        public BotStateSet Use(BotState botState)
        {
            this.botStates.Add(botState);
            return this;
        }

        public async Task OnTurnAsync(ITurnContext context, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            await this.LoadAsync<Dictionary<string, object>>(context, true);
            await next(cancellationToken);
            await this.SaveChangesAsync(context);
        }

        /// <summary>
        /// Load all BotState records in parallel
        /// </summary>
        /// <param name="context"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        public async Task LoadAsync<StateT>(ITurnContext context, bool force = false)
        {
            var tasks = this.botStates.Select(bs => bs.LoadAsync(context)).ToList();
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Save All BotState changes in parallelt
        /// </summary>
        /// <param name="context"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        public async Task SaveChangesAsync(ITurnContext context, bool force = false)
        {
            var tasks = this.botStates.Select(bs => bs.SaveChangesAsync(context)).ToList();
            await Task.WhenAll(tasks);
        }
    }
}
