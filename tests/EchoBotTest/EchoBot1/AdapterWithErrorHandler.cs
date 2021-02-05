// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.11.1

using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EchoBot1
{
    public class AdapterWithErrorHandler : BotFrameworkHttpAdapter, IBotFrameworkHttpAdapter
    {
        private static HttpClient _httpClient = new HttpClient();

        public AdapterWithErrorHandler(IConfiguration configuration, ILogger<BotFrameworkHttpAdapter> logger)
            : base(new ConfigurationCredentialProvider(configuration),  new ConfigurationChannelProvider(configuration), httpClient: _httpClient, logger: logger)
        {
            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                logger.LogError(exception, $"[OnTurnError] unhandled error : {exception.Message}");

                // Send a message to the user
                await turnContext.SendActivityAsync("The bot encountered an error or bug.");
                await turnContext.SendActivityAsync("To continue to run this bot, please fix the bot source code.");

                // Send a trace activity, which will be displayed in the Bot Framework Emulator
                await turnContext.TraceActivityAsync("OnTurnError Trace", exception.Message, "https://www.botframework.com/schemas/error", "TurnError");
            };
        }

        async Task IBotFrameworkHttpAdapter.ProcessAsync(HttpRequest httpRequest, HttpResponse httpResponse, IBot bot, CancellationToken cancellationToken)
        {
            await ProcessAsync(httpRequest, httpResponse, bot, cancellationToken).ConfigureAwait(false);

            System.Diagnostics.Debug.WriteLine(_httpClient.DefaultRequestHeaders.UserAgent);
        }

        public async Task<bool> IsValid(HttpRequest httpRequest, HttpResponse httpResponse)
        {
            var activity = await HttpHelper.ReadRequestAsync<Activity>(httpRequest).ConfigureAwait(false);

            if (string.IsNullOrEmpty(activity?.Type))
            {
                httpResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                return false;
            }

            // Grab the auth header from the inbound http request
            var authHeader = httpRequest.Headers["Authorization"];

            var claimsIdentity = await JwtTokenValidation.AuthenticateRequest(activity, authHeader, CredentialProvider, ChannelProvider).ConfigureAwait(false);
            return true;
        }
    }
}
