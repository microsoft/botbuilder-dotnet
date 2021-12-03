// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

namespace Microsoft.Bot.Builder.TestBot.Shared.Controllers
{
    // This ASP Controller is created to handle a request. Dependency Injection will provide the Adapter and IBot
    // implementation at runtime. Multiple different IBot implementations running at different endpoints can be
    // achieved by specifying a more specific type for the bot constructor argument.
    [Route("api")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private IBot _bot;
        private readonly Func<string, IBot> _service;

        public BotController(IBotFrameworkHttpAdapter adapter, Func<string, IBot> service)
        {
            _adapter = adapter;
            _service = service;
        }

        [Route("{*botname}")]
        [HttpPost]
        [HttpGet]
        public async Task PostAsync(string botname)
        {
            // Delegate the processing of the HTTP POST to the adapter.
            // The adapter will invoke the bot.
            _bot = _service(botname) ?? throw new Exception($"The endpoint '{botname}' is not associated with a bot.");

            await _adapter.ProcessAsync(Request, Response, _bot);
        }
    }
}
