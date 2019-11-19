// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Skills.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;

namespace DialogRootBot.Controllers
{
    /// <summary>
    /// EXPERIMENTAL: This class is just to check if we can provide a ControllerBase instead of manually processing HttpRequests.
    /// </summary>
    [ApiController]
    [Route("v3/conversations/")]
    public class SkillController : ChannelServiceController
    {
        public SkillController(
            ChannelServiceHandler handler,
            ICredentialProvider credentialProvider,
            AuthenticationConfiguration authConfig,
            IChannelProvider channelProvider = null)
            : base(handler, credentialProvider, authConfig, channelProvider)
        {
        }
    }
}
