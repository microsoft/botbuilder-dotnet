// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema;

namespace Microsoft.Bot.Builder.Adapters.WeChat.TestBot
{
    // This ASP Controller is created to handle a request. Dependency Injection will provide the Adapter and IBot
    // implementation at runtime. Multiple different IBot implementations running at different endpoints can be
    // achieved by specifying a more specific type for the bot constructor argument.
    [Route("api/messages")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly IBot _bot;
        private readonly WeChatHttpAdapter _wechatHttpAdapter;

        public BotController(IBot bot, WeChatHttpAdapter wechatAdapter)
        {
            _bot = bot;
            _wechatHttpAdapter = wechatAdapter;
        }

        [HttpGet("/WeChat")]
        [HttpPost("/WeChat")]
        public async Task PostWeChatAsync([FromQuery] SecretInfo postModel)
        {
            // Delegate the processing of the HTTP POST to the adapter.
            // The adapter will invoke the bot.
            await _wechatHttpAdapter.ProcessAsync(Request, Response, _bot, postModel, false);
        }
    }
}
