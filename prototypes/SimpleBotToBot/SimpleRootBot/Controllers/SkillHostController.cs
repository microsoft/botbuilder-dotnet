// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core.Skills;

namespace SimpleRootBot.Controllers
{
    [ApiController]
    [Route("/v3/conversations/{*path}")]
    public class SkillHostController : ControllerBase
    {
        private readonly IBot _bot;
        private readonly BotFrameworkSkillClient _skillClient;

        public SkillHostController(BotFrameworkSkillClient skillClient, IBot bot)
        {
            // adapter to use for calling back to channel
            _bot = bot;
            _skillClient = skillClient;
        }

        [HttpPost]
        [HttpGet]
        [HttpPut]
        [HttpDelete]
        public async Task ProcessAsync()
        {
            try
            {
                // Entering parent from skill
                var authToken = Request.Headers["Authorization"].ToString();

                //DebugTokenClaims();

                // Delegate the processing of the HTTP POST to the adapter.
                // The adapter will invoke the bot.
                await _skillClient.ProcessAsync(Request, Response, _bot);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
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
