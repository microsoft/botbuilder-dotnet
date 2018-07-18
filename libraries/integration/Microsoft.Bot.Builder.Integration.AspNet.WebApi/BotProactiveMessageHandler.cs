// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Integration.AspNet.WebApi.Handlers
{
    public sealed class BotProactiveMessageHandler : BotMessageHandlerBase
    {
        public static readonly string RouteName = "BotFramework - Proactive Message Handler";

        public BotProactiveMessageHandler(BotFrameworkAdapter botFrameworkAdapter) : base(botFrameworkAdapter)
        {
        }

        protected override async Task<InvokeResponse> ProcessMessageRequestAsync(HttpRequestMessage request, BotFrameworkAdapter botFrameworkAdapter, Func<ITurnContext, Task> botCallbackHandler, CancellationToken cancellationToken)
        {
            const string BotAppIdHttpHeaderName = "MS-BotFramework-BotAppId";
            const string BotAppIdQueryStringParameterName = "BotAppId";

            var botAppId = default(string);

            if (request.Headers.TryGetValues(BotAppIdHttpHeaderName, out var botIdHeaders))
            {
                botAppId = botIdHeaders.FirstOrDefault();
            }
            else
            {
                botAppId = request.GetQueryNameValuePairs()
                                .Where(kvp => kvp.Key == BotAppIdQueryStringParameterName)
                                .Select(kvp => kvp.Value)
                                .FirstOrDefault();
            }

            if (string.IsNullOrEmpty(botAppId))
            {
                throw new InvalidOperationException($"Expected a Bot App ID in a header named \"{BotAppIdHttpHeaderName}\" or in a querystring parameter named \"{BotAppIdQueryStringParameterName}\".");
            }

            var conversationReference = await request.Content.ReadAsAsync<ConversationReference>(BotMessageHandlerBase.BotMessageMediaTypeFormatters, cancellationToken);

            await botFrameworkAdapter.ContinueConversationAsync(botAppId, conversationReference, botCallbackHandler, cancellationToken);

            return null;
        }
    }
}
