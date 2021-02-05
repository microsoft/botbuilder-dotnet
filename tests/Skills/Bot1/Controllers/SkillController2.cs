// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Skills;

namespace Microsoft.BotBuilderSamples.SimpleRootBot.Controllers
{
    /// <summary>
    /// A controller that handles skill replies to the bot.
    /// This example uses the <see cref="SkillHandler"/> that is registered as a <see cref="ChannelServiceHandler"/> in startup.cs.
    /// </summary>
    [ApiController]
    [Route("api/skills/extrapath2")]
    public class SkillController2 : ChannelServiceController
    {
        public SkillController2(ChannelServiceHandler handler)
            : base(handler)
        {
        }
    }
}
