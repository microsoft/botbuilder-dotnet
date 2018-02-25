using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Samples.EchoBot_ASPNetCore_DI
{
    /// <summary>
    /// Bot adapter helper.
    /// </summary>
    public class BotAdapterHelper
    {
        /// <summary>
        /// The HTTP context accessor (used to access HttpContext of the current request).
        /// </summary>
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// The bot adapter.
        /// </summary>
        private readonly BotAdapter botAdapter;

        /// <summary>
        /// Initializes a new instance of the <see cref="BotAdapterHelper"/> class.
        /// Use this only when Adapter type is expected to be to BotFramework. To use it with
        /// Dependency injection register HttpContextAccessor as IHttpContextAccessor (singleton instance not per request).
        /// Otherwise just pass a new instance of HttpContextAccessor.
        /// </summary>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <param name="botAdapter">Bot Adapter.<</param>
        public BotAdapterHelper(IHttpContextAccessor httpContextAccessor, BotAdapter botAdapter)
            : this (botAdapter)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BotAdapterHelper"/> class.
        /// </summary>
        public BotAdapterHelper(BotAdapter botAdapter)
        {
            this.botAdapter = botAdapter;
        }

        /// <summary>
        /// Processes the activity. Provides a single function for processing activities thru all 3 adapters.
        /// </summary>
        /// <param name="activity">The activity to be processed. Can be set to null for <see cref="ConsoleAdapter"/>.</param>
        /// <param name="callback">The callback.</param>
        /// <returns>Task tracking operation.</returns>
        public async Task ProcessActivity(IActivity activity, Func<IBotContext, Task> callback)
        {
            if (botAdapter as BotFrameworkAdapter != null)
            {
                await (botAdapter as BotFrameworkAdapter).ProcessActivty(httpContextAccessor.HttpContext.Request.Headers["Authorization"], activity, callback);
            }
            else if (botAdapter as ConsoleAdapter != null)
            {
                await (botAdapter as ConsoleAdapter).ProcessActivity(callback);
            }
            else if (botAdapter as TestAdapter != null)
            {
                await (botAdapter as TestAdapter).ProcessActivity(activity, callback);
            }
        }
    }
}
