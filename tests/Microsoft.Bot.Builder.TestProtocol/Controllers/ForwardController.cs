// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Schema;
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
        private readonly SkillConversationIdFactoryBase _factory;

        public ForwardController(BotFrameworkHttpClient client, IConfiguration configuration, SkillConversationIdFactoryBase factory)
        {
            _client = client;
            _toUri = new Uri(configuration["Next"]);
            _serviceUrl = new Uri(configuration["ServiceUrl"]);
            _factory = factory;
        }

        [HttpPost]
        public async Task PostAsync()
        {
            var inboundActivity = await HttpHelper.ReadRequestAsync<Activity>(Request);
            var nextConversationId = await _factory.CreateSkillConversationIdAsync(inboundActivity.GetConversationReference(), CancellationToken.None);
            await _client.PostActivityAsync(null, null, _toUri, _serviceUrl, nextConversationId, inboundActivity);

            // ALTERNATIVE API IDEA...
            //var inboundConversationReference = inboundActivity.GetConversationReference();
            //var outboundActivity = MessageFactory.CreateActivity(inboundActivity);
            //outboundActivity.ApplyConversationReference(inboundConversationReference, _serviceUrl, nextConversationId);
            //await _client.PostActivityAsync(_toUri, outboundActivity, null, null);
        }
    }
}
