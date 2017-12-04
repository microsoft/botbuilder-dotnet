using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Storage;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Samples.Middleware;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

namespace Microsoft.Bot.Samples.Connector.EchoBot.Controllers
{
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        Builder.Bot _bot;
        BotFrameworkAdapter _adapter; 

        public MessagesController(IConfiguration configuration)
        {
            string appId = configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey)?.Value ?? string.Empty;
            string appKey = configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppPasswordKey).Value ?? string.Empty;

            _adapter = new BotFrameworkAdapter(appId, appKey);
            _bot = new Builder.Bot(_adapter)
                .Use(new FileStorage(System.IO.Path.GetTempPath()))
                .Use(new BotStateManager())
                .Use(new RegExpRecognizerMiddleware()
                    .AddIntent("echoIntent", new Regex("echo (.*)", RegexOptions.IgnoreCase))
                    .AddIntent("helpIntent", new Regex("help (.*)", RegexOptions.IgnoreCase)))
                .Use(new EchoMiddleware())
                .OnReceive( async (context) =>
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
            await _adapter.Receive(HttpContext.Request.Headers, activity);
        }      
    }
}