// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Serialization;

namespace Microsoft.Bot.Builder.Integration.AspNet.WebApi.Handlers
{
    public sealed class BotMessageHandler : BotMessageHandlerBase
    {
        public static readonly string RouteName = "BotFramework - Message Handler";

        public BotMessageHandler(BotFrameworkAdapter botFrameworkAdapter, IActivitySerializer activitySerializer)
            : base(botFrameworkAdapter, activitySerializer)
        {
        }

        protected override async Task<InvokeResponse> ProcessMessageRequestAsync(HttpRequestMessage request, Func<ITurnContext, Task> botCallbackHandler, CancellationToken cancellationToken)
        {
            var activity = await ActivitySerializer.DeserializeAsync(await request.Content.ReadAsStreamAsync(), cancellationToken);

            var invokeResponse = await BotFrameworkAdapter.ProcessActivityAsync(
                request.Headers.Authorization?.ToString(),
                activity,
                botCallbackHandler,
                cancellationToken);

            return invokeResponse;
        }
    }
}
