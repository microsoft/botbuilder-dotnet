// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

namespace DialogRootBot.Controllers
{
    /// <summary>
    /// EXPERIMENTAL: This class is just to check if we can provide a ControllerBase instead of manually processing HttpRequests.
    /// </summary>
    [ApiController]
    [Route("v3/conversations/")]
    public class SkillController : ChannelServiceController
    {
        public SkillController(ChannelServiceHandler handler)
            : base(handler)
        {
        }
    }
}
