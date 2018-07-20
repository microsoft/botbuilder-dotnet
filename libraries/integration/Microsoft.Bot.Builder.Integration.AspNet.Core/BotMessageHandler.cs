// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Serialization;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.Handlers
{
    public class BotMessageHandler : BotMessageHandlerBase
    {
        public BotMessageHandler(IActivitySerializer activitySerializer) : base(activitySerializer)
        {
        }

        protected override async Task<InvokeResponse> ProcessMessageRequestAsync(HttpRequest request, BotFrameworkAdapter botFrameworkAdapter, Func<ITurnContext, Task> botCallbackHandler, CancellationToken cancellationToken)
        {
            var activity = await ActivitySerializer.DeserializeAsync(request.Body, cancellationToken);

            var invokeResponse = await botFrameworkAdapter.ProcessActivityAsync(
                    request.Headers["Authorization"],
                    activity,
                    botCallbackHandler,
                    cancellationToken);

            return invokeResponse;
        }
    }
}
