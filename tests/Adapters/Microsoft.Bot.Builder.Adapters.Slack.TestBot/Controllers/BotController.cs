// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.3.0

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.Bot.Builder.Adapters.Slack.TestBot.Controllers
{
    /// <summary> This ASP Controller is created to handle a request. Dependency Injection will provide the Adapter and IBot
    /// implementation at runtime. Multiple different IBot implementations running at different endpoints can be.
    /// </summary> achieved by specifying a more specific type for the bot constructor argument.
    [Route("api/messages")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly SlackAdapter adapter;
        private readonly IBot bot;

        /// <summary>
        /// Initializes a new instance of the <see cref="BotController"/> class.
        /// </summary>
        /// <param name="adapter">adapter for the BotController.</param>
        /// <param name="bot">bot for the BotController.</param>
        public BotController(SlackAdapter adapter, IBot bot)
        {
            this.adapter = adapter;
            this.bot = bot;
        }

        /// <summary>
        /// PostAsync method that returns an async Task.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpPost]
        public async Task PostAsync()
        {
            // Delegate the processing of the HTTP POST to the adapter.
            // The adapter will invoke the bot.
            await this.adapter.ProcessAsync(this.Request, this.Response, this.bot, new CancellationToken());
        }
    }
}
