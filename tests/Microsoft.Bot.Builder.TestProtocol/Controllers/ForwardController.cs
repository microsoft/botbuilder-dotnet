// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Builder.TestProtocol.Controllers
{
    [Route("api/messages")]
    [ApiController]
    public class ForwardController : ControllerBase
    {
        private readonly BotFrameworkHttpClient _client;
        private readonly Uri _toUri;
        private readonly Uri _serviceUrl;
        private readonly ISkillConversationIdFactory _factory;

        public ForwardController(BotFrameworkHttpClient client, IConfiguration configuration, ISkillConversationIdFactory factory)
        {
            _client = client;
            _toUri = new Uri(configuration["next"]);
            _serviceUrl = new Uri(configuration["serviceUrl"]);
            _factory = factory;
        }

        [HttpPost]
        public async Task PostAsync()
        {
            var inboundActivity = await HttpHelper.ReadRequestAsync(Request);

            var currentConversationId = inboundActivity.Conversation.Id;
            var currentServiceUrl = inboundActivity.ServiceUrl;

            var nextConversationId = _factory.CreateSkillConversationId(currentConversationId, currentServiceUrl);

            await _client.PostActivityAsync(null, null, _toUri, _serviceUrl, nextConversationId, inboundActivity);
        }
    }
}
