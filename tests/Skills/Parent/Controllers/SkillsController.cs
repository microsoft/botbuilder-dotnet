using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

namespace Microsoft.BotBuilderSamples.Controllers
{
    [ApiController]
    [Route("api/skills")]
    public class SkillsController : ChannelServiceController
    {
        public SkillsController(ChannelServiceHandler handler)
            : base(handler)
        {
        }
    }
}
