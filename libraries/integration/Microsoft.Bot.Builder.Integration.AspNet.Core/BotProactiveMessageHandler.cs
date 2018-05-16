// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.Handlers
{
    public class BotProactiveMessageHandler : BotMessageHandlerBase
    {
        protected override async Task<InvokeResponse> ProcessMessageRequestAsync(HttpRequest request, BotFrameworkAdapter botFrameworkAdapter, Func<ITurnContext, Task> botCallbackHandler)
        {
            const string BotAppIdHttpHeaderName = "MS-BotFramework-BotAppId";
            const string BotIdQueryStringParameterName = "BotAppId";

            if (!request.Headers.TryGetValue(BotAppIdHttpHeaderName, out var botAppIdHeaders))
            {
                if (!request.Query.TryGetValue(BotIdQueryStringParameterName, out botAppIdHeaders))
                {
                    throw new InvalidOperationException($"Expected a Bot App ID in a header named \"{BotAppIdHttpHeaderName}\" or in a querystring parameter named \"{BotIdQueryStringParameterName}\".");
                }
            }

            var botAppId = botAppIdHeaders.First();
            var conversationReference = default(ConversationReference);

            using (var bodyReader = new JsonTextReader(new StreamReader(request.Body, Encoding.UTF8)))
            {
                conversationReference = BotMessageHandlerBase.BotMessageSerializer.Deserialize<ConversationReference>(bodyReader);
            }

            await botFrameworkAdapter.ContinueConversation(botAppId, conversationReference, botCallbackHandler);

            return null;
        }
    }
}