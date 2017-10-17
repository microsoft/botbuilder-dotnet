using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Storage;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Samples.Middleware;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Samples.Connector.EchoBot.Controllers
{
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        Builder.Bot _bot; 

        public MessagesController()
        {
            var adapter = new BotFrameworkAdapter("", "");

            _bot = new Builder.Bot(adapter)
                .Use(new FileStorage(System.IO.Path.GetTempPath()))
                .Use(new BotStateManager())
                .Use(new RegExpRecognizerMiddleware()
                    .AddIntent("echoIntent", new Regex("echo (.*)", RegexOptions.IgnoreCase))
                    .AddIntent("helpIntent", new Regex("help (.*)", RegexOptions.IgnoreCase)))
                .Use(new EchoMiddleware())
                .OnReceive( async (context, token) =>
                    {
                        // Example of handling the Help intent w/o using Middleware
                        if (context.IfIntent("helpIntent"))                            
                        {                            
                            context.Reply("Ask this bot to 'Echo something' and it will!");                                
                            return new ReceiveResponse(true);
                        }
                        return new ReceiveResponse(false);
                    }
                );
        }

        [HttpPost]
        public async void Post([FromBody]Activity activity)
        {
            BotFrameworkAdapter connector = (BotFrameworkAdapter)_bot.Adapter; 
            await connector.Receive(HttpContext.Request.Headers, activity, CancellationToken.None);            
        }      
    }
}