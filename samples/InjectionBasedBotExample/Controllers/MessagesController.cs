using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using System.Threading;
using Microsoft.Bot.Builder.Adapters;

namespace InjectionBasedBotExample.Controllers
{
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        private readonly Bot _bot;

        public MessagesController(Bot b)
        {
            _bot = b;
        }
        
        [HttpPost]
        public async void Post([FromBody]Activity activity)
        {
            BotFrameworkAdapter connector = (BotFrameworkAdapter)_bot.Adapter;
            await connector.Receive(HttpContext.Request.Headers, activity, CancellationToken.None);
        }
    }
}
