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
    public sealed class BotExternalEventsHandler : BotMessageHandlerBase
    {
        public static readonly string RouteName = "BotFramework - External Event Handler";

        public BotExternalEventsHandler(BotFrameworkAdapter botFrameworkAdapter) : base(botFrameworkAdapter)
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

            var eventActivity = await request.Content.ReadAsAsync<Activity>(BotMessageHandlerBase.BotMessageMediaTypeFormatters, cancellationToken);

            if (eventActivity?.Type == null)
            {
                throw new InvalidOperationException($"Expected to find an activity of type \"{ActivityTypes.Event}\" in the request body.");
            }

            await botFrameworkAdapter.ProcessActivityAsync(botAppId, eventActivity, botCallbackHandler, cancellationToken);

            return null;
        }
    }
}
