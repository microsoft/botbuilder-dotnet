// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Builder.Adapters.WeChat.TestBot
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
        private readonly IWeChatHttpAdapter _weChatHttpAdapter;
        private readonly string _token;

        public BotController(IBotFrameworkHttpAdapter adapter, IBot bot, IWeChatHttpAdapter weChatAdapter, IConfiguration configuration)
        {
            _adapter = adapter;
            _bot = bot;
            _weChatHttpAdapter = weChatAdapter;
            _token = configuration.GetSection("WeChatSetting").GetSection("Token").Value;
        }

        [HttpPost]
        public async Task PostAsync()
        {
            // Delegate the processing of the HTTP POST to the adapter.
            // The adapter will invoke the bot.
            await _adapter.ProcessAsync(Request, Response, _bot);
        }

        [HttpPost("/WeChat")]
        public async Task PostWeChatAsync([FromQuery] SecretInfo postModel)
        {
            // Delegate the processing of the HTTP POST to the adapter.
            // The adapter will invoke the bot.
            await _weChatHttpAdapter.ProcessAsync(Request, Response, _bot, postModel, false);
        }

        // GET: api/messages
        [HttpGet("/WeChat")]
        public ActionResult Get(string echostr, [FromQuery] SecretInfo postModel)
        {
            try
            {
                VerificationHelper.Check(postModel.Signature, postModel.Timestamp, postModel.Nonce, _token);
                return Content(echostr);
            }
            catch
            {
                return Content("failed:" + postModel.Signature);
            }
        }
    }
}
