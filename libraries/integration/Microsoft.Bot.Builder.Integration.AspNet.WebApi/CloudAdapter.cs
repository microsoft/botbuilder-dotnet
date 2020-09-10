// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
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
        /// Initializes a new instance of the <see cref="CloudAdapter"/> class.
        /// </summary>
        /// <param name="cloudEnvironment">The cloud environment used for validating and creating tokens.</param>
        /// <param name="httpClient">The HttpClient implementation this adapter should use.</param>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        public CloudAdapter(
            ICloudEnvironment cloudEnvironment,
            ILogger logger = null,
            HttpClient httpClient = null)
            : base(cloudEnvironment, logger, httpClient)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudAdapter"/> class.
        /// </summary>
        /// <param name="httpClient">The HttpClient implementation this adapter should use.</param>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        public CloudAdapter(
            ILogger logger = null,
            HttpClient httpClient = null)
            : base(new ConfigurationCloudEnvironment(), logger, httpClient)
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
                    // process the inbound activity with the bot
                    var invokeResponse = await ProcessActivityAsync(authHeader, activity, bot.OnTurnAsync, cancellationToken).ConfigureAwait(false);

                    // write the response, potentially serializing the InvokeResponse
                    HttpHelper.WriteResponse(httpRequest, httpResponse, invokeResponse);
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
