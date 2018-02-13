// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.BotFramework;
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
        public async Task<IActionResult> Post([FromBody]Activity activity)
        {
            try
            {
                await ((BotFrameworkAdapter)_bot.Adapter).Receive(this.Request.Headers, activity);
                return this.Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return this.Unauthorized();
            }
        }
    }
}
