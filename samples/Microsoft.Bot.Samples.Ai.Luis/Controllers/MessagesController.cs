
// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Ai;
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
                    .Use(new LuisRecognizerMiddleware("xxxxxx", "xxxxxx")
                        .HandleIntent("MyFirstIntent", HandleMyFirstIntent)
                        .HandleIntent("MySecondIntent", HandleMySecondIntent));
                
                // becuase by default routing of an activity is terminated when an intent is handled 
                // you can can use the LuisRecognizerMiddleware in a chain, to fallback to another 
                // middleware such as the QnA Maker Middleware or another instance of the Luis Recognizer   

                // LUIS with correct baseUri format example
                //.Use(new LuisRecognizerMiddleware("xxxxxx", "xxxxxx", "https://xxxxxx.api.cognitive.microsoft.com/luis/v2.0/apps"))
            }
        }

        private Task HandleMySecondIntent(IBotContext context, Intent intent)
        {
            context.Reply($"You hit the intent name 'MyFirstIntent' with a score of {intent.Score}");
            return Task.CompletedTask;
        }

        private Task HandleMyFirstIntent(IBotContext context, Intent intent)
        {
            context.Reply($"You hit the intent name 'MySecondIntent' with a score of {intent.Score}");
            return Task.CompletedTask;
        }

        private Task BotReceiveHandler(IBotContext context)
        {
            // No matching intents were returned, or no handler existed for it (e.g. the "None" intent was returned)
            // so activity routing continued to this point. Alternatively we could have added a handler for the "None"
            // intent. i.e..HandleIntent("None", HandleNoneIntent));
            context.Reply("Sorry, not sure what you wanted.");
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
