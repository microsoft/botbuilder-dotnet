// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Samples.CustomMiddleware
{
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        private static BotFrameworkAdapter adapter;

        /// <summary>
        /// In this sample Bot, a new instance of the Bot is created by the controller 
        /// on every incoming HTTP reques. The bot is constructed using the credentials
        /// found in the config file. Note that no credentials are needed if testing
        /// the bot locally using the emulator. 
        /// </summary>        
        public MessagesController(IConfiguration configuration)
        {
            if (adapter == null)
            {
                adapter = new BotFrameworkAdapter(configuration)
                    .Use(new ExampleMiddleware("X"))
                    .Use(new ExampleMiddleware("\tY"))
                    .Use(new ExampleMiddleware("\t\tZ"));

            }
        }

        private Task BotReceiveHandler(IBotContext context)
        {
            if (context.Request.Type == ActivityTypes.Message)
            {
                context.Reply("hello");
            }
            return Task.CompletedTask;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Activity activity)
        {
            try
            {
                await adapter.ProcessActivty(this.Request.Headers["Authorization"].FirstOrDefault(), activity, BotReceiveHandler);
                return this.Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return this.Unauthorized();
            }
        }
    }
}