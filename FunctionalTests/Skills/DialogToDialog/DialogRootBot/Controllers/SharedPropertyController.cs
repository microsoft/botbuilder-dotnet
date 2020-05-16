using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Skills;

namespace Microsoft.BotBuilderSamples.DialogRootBot.Controllers
{
    [ApiController]
    [Route("api/skills")]
    public class SharedPropertyController : UserSharedDataController
    {
        public SharedPropertyController(ChannelServiceHandler handler, BotAdapter botAdapter, UserState userState, SkillConversationIdFactoryBase conversationIdFactory)
            : base(handler, botAdapter, userState, conversationIdFactory)
        {
        }

        /// <summary>
        /// StateAsync.
        /// </summary>
        /// <returns>Queries User State and return.</returns>
        [HttpPost("state")]
        [HttpGet("state")]
        public override async Task StateAsync()
        {
            try
            {
                await base.StateAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
    }
}
