// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Integration.AspNet.WebApi.Handlers
{
    public sealed class BotProactiveMessageHandler : BotMessageHandlerBase
    {
        public BotProactiveMessageHandler(BotFrameworkAdapter botFrameworkAdapter) : base(botFrameworkAdapter)
        {
        }

        protected override async Task ProcessMessageRequestAsync(HttpRequestMessage request, BotFrameworkAdapter botFrameworkAdapter, Func<ITurnContext, Task> botCallbackHandler, CancellationToken cancellationToken)
        {
            var conversationReference = await request.Content.ReadAsAsync<ConversationReference>(BotMessageHandlerBase.BotMessageMediaTypeFormatters, cancellationToken);

            await botFrameworkAdapter.ContinueConversation(conversationReference, botCallbackHandler);
        }
    }
}