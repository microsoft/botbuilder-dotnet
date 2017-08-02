using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Bot.Samples.Connector.EchoBot.Controllers
{
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        private readonly Builder.Bot bot; 

        public MessagesController(Builder.Bot bot)
        {
            this.bot = bot;
            bot.Use(new EchoMiddleWare());

            // instead of using middleware you can register the default OnReceive handler of the bot 
            /*bot.OnReceive = async (context, token) =>
            {
                var activity = context.Request as Activity;
                var reply = activity.CreateReply();
                if (activity.Type == ActivityTypes.Message)
                {
                    reply.Text = $"echo: {activity.Text}";
                }
                else
                {
                    reply.Text = $"activity type: {activity.Type}";
                }
                context.Responses.Add(reply);
                await context.PostAsync(token);
                return true; 
            };*/
        }

        //[Authorize(Roles = "Bot")]
        // POST api/values
        [HttpPost]
        public virtual async Task<OkResult> Post([FromBody]Activity activity,[FromServices]IConnector connector)
        {
            await connector.Receive(HttpContext.Request.Headers, activity, CancellationToken.None);
            return Ok();
        }
    }
}
