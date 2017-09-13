using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Samples.Middleware;
using System.Threading;

namespace Microsoft.Bot.Samples.Connector.EchoBot.Controllers
{
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        Builder.Bot _bot; 

        public MessagesController()
        {
            var connector = new BotFrameworkConnector("", "");

            _bot = new Builder.Bot(connector)
                .Use(new EchoMiddleWare());
        }

        [HttpPost]
        public async void Post([FromBody]Activity activity)
        {
            BotFrameworkConnector connector = (BotFrameworkConnector)_bot.Connector; 
            await connector.Receive(HttpContext.Request.Headers, activity, CancellationToken.None);
            return;
        }      
    }
}