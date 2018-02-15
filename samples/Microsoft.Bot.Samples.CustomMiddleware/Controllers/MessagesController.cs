// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Samples.CustomMiddleware
{
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        private BotFrameworkAdapter _adapter;

        /// <summary>
        /// In this sample Bot, a new instance of the Bot is created by the controller 
        /// on every incoming HTTP reques. The bot is constructed using the credentials
        /// found in the config file. Note that no credentials are needed if testing
        /// the bot locally using the emulator. 
        /// </summary>        
        public MessagesController(IConfiguration configuration)
        {
            var bot = new Builder.Bot(new BotFrameworkAdapter(configuration))
                .Use(new ExampleMiddleware("X"))
                .Use(new ExampleMiddleware("\tY"))
                .Use(new ExampleMiddleware("\t\tZ"));
            bot.OnReceive(BotReceiveHandler);

            _adapter = (BotFrameworkAdapter)bot.Adapter;
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
        public Task Post() => _adapter.Receive(this.Request);
    }
}