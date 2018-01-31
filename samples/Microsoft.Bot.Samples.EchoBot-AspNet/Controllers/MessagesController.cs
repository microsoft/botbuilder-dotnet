// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Samples.EchoBot.Controllers
{
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        Builder.Bot _bot;

        /// <summary>
        /// In this Echo Bot, a new instance of the Bot is created by the controller 
        /// on every incoming HTTP reques. The bot is constructed using the credentials
        /// found in the config file. Note that no credentials are needed if testing
        /// the bot locally using the emulator. 
        ///         
        /// The bot itself simply echoes any messages sent, using the OnReceive 
        /// handler. This handler checks the activity type, and returns
        /// back the sent text. 
        /// </summary>        
        public MessagesController(IConfiguration configuration)
        {            
            string appId = configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey)?.Value ?? string.Empty;
            string appKey = configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppPasswordKey).Value ?? string.Empty;

            _bot = new Builder.Bot(new BotFrameworkAdapter(appId, appKey));
            _bot.OnReceive(async (context, next) =>
            {
                if (context.Request.Type == ActivityTypes.Message)
                {
                    context.Reply($"echo: {context.Request.AsMessageActivity().Text}");
                }
                await next();
            });
        }

        [HttpPost]
        public async void Post([FromBody]Activity activity)
        {
            await ((BotFrameworkAdapter)_bot.Adapter)
                .Receive(HttpContext.Request.Headers, activity);
        }
    }
}