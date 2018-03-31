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
    internal sealed class BotMessageHandler : BotMessageHandlerBase
    {
        private static HttpClient httpClient = new HttpClient();

        public BotMessageHandler(BotFrameworkAdapter botFrameworkAdapter) : base(botFrameworkAdapter)
        {
        }

        protected override async Task ProcessMessageRequestAsync(HttpRequestMessage request, BotFrameworkAdapter botFrameworkAdapter, Func<ITurnContext, Task> botCallbackHandler, CancellationToken cancellationToken)
        {
            var activity = await request.Content.ReadAsAsync<Activity>(BotMessageHandlerBase.BotMessageMediaTypeFormatters, cancellationToken);

            var authContext = await AuthenticationHelper.GetRequestAuthenticationContextAsync(request.Headers.Authorization.ToString(), httpClient);
            AuthenticationHelper.SetRequestAuthenticationContext(authContext);

            await botFrameworkAdapter.ProcessActivity(
                activity,
                botCallbackHandler);
        }
    }
}