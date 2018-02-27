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
    /// Delegate to process a given activity.
    /// </summary>
    /// <param name="botAdapter">Bot adapeter.</param>
    /// <param name="activity">The activity to process.</param>
    /// <param name="callback">The callback to call to process the activity.</param>
    /// <returns>Task tracking operartion</returns>
    public delegate Task ProcessActivity(BotAdapter botAdapter, IActivity activity, Func<IBotContext, Task> callback);

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

        private ProcessActivity processActivityDelegate;

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

            // Register default adapter implementations.
            if (botAdapter as BotFrameworkAdapter != null)
            {
                this.processActivityDelegate = this.ProcessBotFrameworkAdapterRequest;
            }
            else if (botAdapter as ConsoleAdapter != null)
            {
                this.processActivityDelegate = this.ProcessConsoleAdapterRequest;
            }
            else if (botAdapter as TestAdapter != null)
            {
                this.processActivityDelegate = this.ProcessTestAdapterRequest;
            }
        }

        /// <summary>
        /// Registers the activity processor. This is required for Adapter other than
        /// <see cref="BotFrameworkAdapter"/>, <see cref="TestAdapter"/> and <see cref="ConsoleAdapter"/>. These
        /// adapters have default implementation.
        /// </summary>
        /// <param name="processActivityDelegate">The process activity delegate.</param>
        public void RegisterActivityProcessor(ProcessActivity processActivityDelegate)
        {
            this.processActivityDelegate = processActivityDelegate;
        }

        /// <summary>
        /// Processes the activity. Provides a single function for processing activities thru all 3 adapters.
        /// </summary>
        /// <param name="activity">The activity to be processed. Can be set to null for <see cref="ConsoleAdapter"/>.</param>
        /// <param name="callback">The callback.</param>
        /// <returns>Task tracking operation.</returns>
        public async Task ProcessActivity(IActivity activity, Func<IBotContext, Task> callback)
        {
            if (this.processActivityDelegate == null)
            {
                throw new ArgumentNullException(nameof(processActivityDelegate), "No Activity processors are registered");
            }

            await this.processActivityDelegate.Invoke(this.botAdapter, activity, callback);
        }

        /// <summary>
        /// Processes the bot framework adapter request.
        /// </summary>
        /// <param name="botAdapter">The bot adapter.</param>
        /// <param name="activity">The activity.</param>
        /// <param name="callback">The callback.</param>
        /// <returns>Task tracking operation.</returns>
        private async Task ProcessBotFrameworkAdapterRequest(BotAdapter botAdapter, IActivity activity, Func<IBotContext, Task> callback)
        {
            await(botAdapter as BotFrameworkAdapter).ProcessActivty(this.httpContextAccessor.HttpContext.Request.Headers["Authorization"], activity, callback);
        }

        /// <summary>
        /// Processes the console adapter request.
        /// </summary>
        /// <param name="botAdapter">The bot adapter.</param>
        /// <param name="activity">The activity.</param>
        /// <param name="callback">The callback.</param>
        /// <returns>Task tracking request.</returns>
        private async Task ProcessConsoleAdapterRequest(BotAdapter botAdapter, IActivity activity, Func<IBotContext, Task> callback)
        {
            await (botAdapter as ConsoleAdapter).ProcessActivity(callback);
        }

        /// <summary>
        /// Processes the test adapter request.
        /// </summary>
        /// <param name="botAdapter">The bot adapter.</param>
        /// <param name="activity">The activity.</param>
        /// <param name="callback">The callback.</param>
        /// <returns>Task tracking operation.</returns>
        private async Task ProcessTestAdapterRequest(BotAdapter botAdapter, IActivity activity, Func<IBotContext, Task> callback)
        {
            await (botAdapter as TestAdapter).ProcessActivity(activity, callback);
        }
    }
}
