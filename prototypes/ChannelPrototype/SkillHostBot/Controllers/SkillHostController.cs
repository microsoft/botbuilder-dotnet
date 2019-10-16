// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Skills.Preview.Integration;

namespace SkillHost.Controllers
{
    [ApiController]
    [Route("/v3/conversations/{*path}")]
    public class SkillHostController : ControllerBase
    {
        private readonly IBot _bot;
        private readonly BotFrameworkHttpSkillsServer _skillServer;

        public SkillHostController(BotFrameworkHttpSkillsServer skillServer, IBot bot)
        {
            // adapter to use for calling back to channel
            _bot = bot;
            _skillServer = skillServer;
        }

        [HttpPost]
        [HttpGet]
        [HttpPut]
        [HttpDelete]
        public async Task ProcessAsync()
        {
            // Entering parent from skill
            var authToken = Request.Headers["Authorization"].ToString();

            // Delegate the processing of the HTTP POST to the adapter.
            // The adapter will invoke the bot.
            await _skillServer.ProcessAsync(Request, Response, _bot);
        }
    }
}
