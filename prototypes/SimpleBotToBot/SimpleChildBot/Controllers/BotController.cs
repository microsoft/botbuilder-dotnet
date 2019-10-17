// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

namespace SimpleChildBot.Controllers
{
    // This ASP Controller is created to handle a request. Dependency Injection will provide the Adapter and IBot
    // implementation at runtime. Multiple different IBot implementations running at different endpoints can be
    // achieved by specifying a more specific type for the bot constructor argument.
    [Route("api/messages")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly IBot _bot;

        public BotController(IBotFrameworkHttpAdapter adapter, IBot bot)
        {
            _adapter = adapter;
            _bot = bot;
        }

        [HttpPost]
        public async Task PostAsync()
        {
            // Entering skill
            var authToken = Request.Headers["Authorization"].ToString();
					
            // DebugTokenClaims();

            // Delegate the processing of the HTTP POST to the adapter.
            // The adapter will invoke the bot.
            await _adapter.ProcessAsync(Request, Response, _bot);
        }

                /// <summary>
        /// Helper to debug claims returned by skill.
        /// </summary>
        private void DebugTokenClaims()
        {
            var authToken = Request.Headers["Authorization"];
            var jwtToken = new JwtSecurityToken(authToken.ToString().Split(' ')[1]);
            foreach (var claim in jwtToken.Claims)
            {
                Console.WriteLine($"Claim: {claim.Type} Value: {claim.Value}");
            }
        }
    }
}
