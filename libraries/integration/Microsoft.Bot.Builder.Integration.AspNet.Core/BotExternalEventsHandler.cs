// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.Handlers
{
    public class BotExternalEventsHandler : BotMessageHandlerBase
    {
        public BotExternalEventsHandler()
        {
        }

        protected override async Task<InvokeResponse> ProcessMessageRequestAsync(HttpRequest request, BotFrameworkAdapter botFrameworkAdapter, Func<ITurnContext, Task> botCallbackHandler, CancellationToken cancellationToken)
        {
            ClaimsIdentity botIdentity = GetBotIdentityFromRequest();
            Activity eventActivity = GetEventActivityFromRequest();

            await botFrameworkAdapter.ProcessActivityAsync(botIdentity, eventActivity, botCallbackHandler, cancellationToken);

            return null;

            ClaimsIdentity GetBotIdentityFromRequest()
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

                var result = new ClaimsIdentity("Bot-ExternalEvent-Auth");
                result.AddClaim(new Claim(AuthenticationConstants.AudienceClaim, botAppId));
                result.AddClaim(new Claim(AuthenticationConstants.AppIdClaim, botAppId));

                return result;
            }

            Activity GetEventActivityFromRequest()
            {
                var result = default(Activity);

                if (request.ContentLength > 0)
                {
                    using (var bodyReader = new JsonTextReader(new StreamReader(request.Body, Encoding.UTF8)))
                    {
                        result = BotMessageHandlerBase.BotMessageSerializer.Deserialize<Activity>(bodyReader);
                    }
                }

                if (result?.Type != ActivityTypes.Event)
                {
                    throw new InvalidOperationException($"Expected to find an activity of type \"{ActivityTypes.Event}\" in the request body.");
                }

                return result;
            }
        }
    }
}
