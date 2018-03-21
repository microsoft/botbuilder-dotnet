// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.Handlers
{
    public class BotProactiveMessageHandler : BotMessageHandlerBase
    {
        public BotProactiveMessageHandler(BotFrameworkAdapter botFrameworkAdapter) : base(botFrameworkAdapter)
        {
        }
       
        protected override async Task ProcessMessageRequestAsync(HttpRequest request, BotFrameworkAdapter botFrameworkAdapter, Func<ITurnContext, Task> botCallbackHandler)
        {
            var conversationReference = default(ConversationReference);

            using (var bodyReader = new JsonTextReader(new StreamReader(request.Body, Encoding.UTF8)))
            {
                conversationReference = BotMessageHandlerBase.BotMessageSerializer.Deserialize<ConversationReference>(bodyReader);
            }

            await botFrameworkAdapter.ContinueConversation(conversationReference, botCallbackHandler);
        }
    }
}