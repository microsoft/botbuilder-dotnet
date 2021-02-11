// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

namespace Microsoft.Bot.Builder.TestProtocol.Controllers
{
    [Route("api/connector")]
    [ApiController]
    public class BackwardController : ChannelServiceController
    {
        public BackwardController(ChannelServiceHandler handler)
            : base(handler)
        {
        }
    }
}
