// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder.Integration.AspNet.WebApi
{
    /// <summary>
    /// An adapter that implements the Bot Framework Protocol and can be hosted in different cloud environmens both public and private.
    /// </summary>
    public class CloudAdapter : CloudAdapterBase, IBotFrameworkHttpAdapter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CloudAdapter"/> class. (Public cloud. No auth. For testing.)
        /// </summary>
        public CloudAdapter()
            : this(BotFrameworkAuthenticationFactory.Create(null, false, null, null, null, null, null, null, null, new PasswordServiceClientCredentialFactory(), new AuthenticationConfiguration(), null, null))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudAdapter"/> class.
        /// </summary>
        /// <param name="botFrameworkAuthentication">The cloud environment used for validating and creating tokens.</param>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        public CloudAdapter(
            BotFrameworkAuthentication botFrameworkAuthentication,
            ILogger logger = null)
            : base(botFrameworkAuthentication, logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudAdapter"/> class.
        /// </summary>
        /// <param name="httpClient">The HttpClient implementation this adapter should use.</param>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        public CloudAdapter(
            HttpClient httpClient = null,
            ILogger logger = null)
            : this(new ConfigurationBotFrameworkAuthentication(httpClient: httpClient, logger: logger), logger)
        {
        }

        /// <inheritdoc/>
        public async Task ProcessAsync(HttpRequestMessage httpRequest, HttpResponseMessage httpResponse, IBot bot, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (httpRequest == null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }

            if (httpResponse == null)
            {
                throw new ArgumentNullException(nameof(httpResponse));
            }

            if (bot == null)
            {
                throw new ArgumentNullException(nameof(bot));
            }

            if (httpRequest.Method == HttpMethod.Get)
            {
                throw new NotImplementedException("web sockets is not yet implemented");
            }
            else
            {
                // deserialize the incoming Activity
                var activity = await HttpHelper.ReadRequestAsync(httpRequest, cancellationToken).ConfigureAwait(false);

                if (string.IsNullOrEmpty(activity?.Type))
                {
                    httpResponse.StatusCode = HttpStatusCode.BadRequest;
                    return;
                }

                // grab the auth header from the inbound http request
                var authHeader = httpRequest.Headers.Authorization?.ToString();

                try
                {
                    if (activity.DeliveryMode == DeliveryModes.ExpectReplies || activity.Type == ActivityTypes.Invoke)
                    {
                        // process the inbound activity with the bot
                        var invokeResponse = await ProcessActivityAsync(authHeader, activity, bot.OnTurnAsync, cancellationToken).ConfigureAwait(false);

                        // write the response, potentially serializing the InvokeResponse
                        HttpHelper.WriteResponse(httpRequest, httpResponse, invokeResponse);
                    }
                    else
                    {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        // run pipeline in background and return immediately.
                        var turnProcessingTask = ProcessActivityAsync(authHeader, activity, bot.OnTurnAsync, cancellationToken)
                            .ContinueWith(t => t?.Exception?.Handle((e) => true), TaskScheduler.Default);

                        // when in asp.net we tell it about the background task 
                        if (HostingEnvironment.IsHosted)
                        {
                            HostingEnvironment.QueueBackgroundWorkItem((ct) => turnProcessingTask);
                        }
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        httpResponse.StatusCode = HttpStatusCode.Accepted;
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    // handle unauthorized here as this layer creates the http response
                    httpResponse.StatusCode = HttpStatusCode.Unauthorized;
                }
            }
        }
    }
}
