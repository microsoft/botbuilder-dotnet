using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;

namespace EchoBot.Controllers
{
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        [HttpPost]
        public async void Post([FromBody]Activity activity)
        {
            try
            {
                string appId = "";
                string appPassword = "";
                var adapter = new BotFrameworkAdapter(appId, appPassword);
                var bot = new Bot(adapter)
                    .OnReceive(async (context, next) =>
                    {
                        if (context.Request.Type == ActivityTypes.Message)
                        {
                            context.Reply($"echo: {context.Request.AsMessageActivity().Text}");
                        }
                        await next();
                    });

                // We have an activity from the user, give it to the adapter->Bot 
                await adapter.Receive(HttpContext.Request.Headers, activity);
            }
            catch (UnauthorizedAccessException err)
            {
                // sender is not authorized to call us
                HttpContext.Response.StatusCode = 403;
            }
            catch(MicrosoftAppCredentials.OAuthException err)
            {
                // we failed to send post message back because our credentials 
                System.Diagnostics.Trace.TraceError(err.Message);
            }
        }
    }
}
