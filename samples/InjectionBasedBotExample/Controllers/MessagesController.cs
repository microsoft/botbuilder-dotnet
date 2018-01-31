using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;

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
            await connector.Receive(HttpContext.Request.Headers, activity);
        }
    }
}
