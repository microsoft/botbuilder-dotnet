// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Ai;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Samples.Ai.QnA.Controllers
{
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        BotFrameworkAdapter _adapter;
        public MessagesController(IConfiguration configuration)
        {
            var qnaOptions = new QnAMakerOptions
            {
                // add subscription key and knowledge base id
                SubscriptionKey = "xxxxxx",
                KnowledgeBaseId = "xxxxxx"
            };
            var bot = new Builder.Bot(new BotFrameworkAdapter(configuration))
                // add QnA middleware 
                .Use(new QnAMaker(qnaOptions))
                .OnReceive(BotReceiveHandler);
               
            _adapter = (BotFrameworkAdapter)bot.Adapter;
        }

        private Task BotReceiveHandler(IBotContext context, MiddlewareSet.NextDelegate next)
        {
            if (context.Request.Type == ActivityTypes.Message && context.Responses.Count == 0)
            {
                // add app logic when QnA Maker doesn't find an answer
                context.Reply("No good match found in the KB.");
            }
            return Task.CompletedTask;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Activity activity)
        {
            try
            {
                await _adapter.Receive(this.Request.Headers["Authorization"].FirstOrDefault(), activity);
                return this.Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return this.Unauthorized();
            }
        }
    }
}
