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
    public sealed class BotMessageHandler : BotMessageHandlerBase
    {
        internal static readonly string RouteName = "BotFramework - Message Handler";

        public BotMessageHandler(BotFrameworkAdapter botFrameworkAdapter) : base(botFrameworkAdapter)
        {
        }

        protected override async Task<InvokeResponse> ProcessMessageRequestAsync(HttpRequestMessage request, BotFrameworkAdapter botFrameworkAdapter, Func<ITurnContext, Task> botCallbackHandler, CancellationToken cancellationToken)
        {
            var activity = await request.Content.ReadAsAsync<Activity>(BotMessageHandlerBase.BotMessageMediaTypeFormatters, cancellationToken);

            var invokeResponse = await botFrameworkAdapter.ProcessActivity(
                request.Headers.Authorization?.ToString(),
                activity,
                botCallbackHandler);

            return invokeResponse;
        }
    }
}