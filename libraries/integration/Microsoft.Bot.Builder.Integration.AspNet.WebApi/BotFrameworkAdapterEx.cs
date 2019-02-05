// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;

namespace Microsoft.Bot.Builder.Integration.AspNet.WebApi
{
    /// <summary>
    /// A Bot Builder Adapter implementation used to handled bot Framework HTTP requests.
    /// </summary>
    public class BotFrameworkAdapterEx : BotFrameworkAdapter, IBotFrameworkAdapter
    {
        public BotFrameworkAdapterEx(ICredentialProvider credentialProvider)
            : base(credentialProvider)
        {
        }

        public async Task ProcessAsync(HttpRequestMessage request, HttpResponseMessage response, IBot bot, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            if (bot == null)
            {
                throw new ArgumentNullException(nameof(bot));
            }

            // deserialize the incoming Activity
            var activity = await HttpHelper.FromRequestAsync(request, cancellationToken).ConfigureAwait(false);

            // grab the auth header from the inbound http request
            var authHeader = request.Headers.Authorization?.ToString();

            // process the inbound activity with the bot
            var invokeResponse = await ProcessActivityAsync(authHeader, activity, bot.OnTurnAsync, cancellationToken).ConfigureAwait(false);

            // write the response, potentially serializing the InvokeResponse
            HttpHelper.ToResponse(request, response, invokeResponse);
        }
    }
}
