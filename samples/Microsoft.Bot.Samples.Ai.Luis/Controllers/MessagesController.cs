
// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Ai;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Samples.Ai.Luis
{
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        static BotFrameworkAdapter adapter;

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
                    .Use(new LuisRecognizerMiddleware("xxxxxx", "xxxxxx"));

                // LUIS with correct baseUri format example
                //.Use(new LuisRecognizerMiddleware("xxxxxx", "xxxxxx", "https://xxxxxx.api.cognitive.microsoft.com/luis/v2.0/apps"))

            }
        }

        private Task BotReceiveHandler(IBotContext context)
        {
            if (context.Request.Type == ActivityTypes.Message)
            {
                context.Reply($"the top intent was: {context.TopIntent.Name}");

                foreach (var entity in context.TopIntent.Entities)
                {
                    context.Reply($"entity: {entity.ValueAs<string>()}");
                }
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
