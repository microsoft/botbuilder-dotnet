// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.WebSockets;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.StreamingExtensions.Transport;
using Microsoft.Bot.StreamingExtensions.Transport.WebSockets;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder.Integration.AspNet.WebApi.StreamingExtensions
{
    internal class WebSocketConnector
    {
        // These headers are used to send the required values for validation of an incoming connection request from an ABS channel.
        // TODO: We must document this somewhere, right? Find it and put a reference link here.
        private const string AuthHeaderName = "authorization";
        private const string ChannelIdHeaderName = "channelid";
        private readonly IChannelProvider _channelProvider;
        private readonly ICredentialProvider _credentialProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketConnector"/> class.
        /// Constructor for use when establishing a connection with a WebSocket server.
        /// </summary>
        /// <param name="credentialProvider">Used for validating channel credential authentication information.</param>
        /// <param name="channelProvider">Used for validating channel authentication information.</param>
        /// <param name="logger">Set in order to enable logging.</param>
        internal WebSocketConnector(ICredentialProvider credentialProvider, IChannelProvider channelProvider = null)
        {
            _credentialProvider = credentialProvider;
            _channelProvider = channelProvider;
        }

        /// <summary>
        /// Process the initial request to establish a long lived connection via a streaming server.
        /// </summary>
        /// <param name="onTurnError">Logic to execute on encountering turn errors.</param>
        /// <param name="middlewareSet">The set of middleware to perform on each turn.</param>
        /// <param name="httpRequest">The connection request.</param>
        /// <param name="httpResponse">The response sent on error or connection termination.</param>
        /// <param name="bot">The bot that is communicating over this connection.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>Returns on task completion.</returns>
        internal async Task ProcessAsync(Func<ITurnContext, Exception, Task> onTurnError, List<IMiddleware> middlewareSet, HttpRequestMessage httpRequest, HttpResponseMessage httpResponse, IBot bot = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (httpRequest == null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }

            if (httpResponse == null)
            {
                throw new ArgumentNullException(nameof(httpResponse));
            }

            if (!System.Web.HttpContext.Current.IsWebSocketRequest)
            {
                httpResponse.Content = new StringContent("Upgrade to web socket is required");
                httpResponse.StatusCode = HttpStatusCode.BadRequest;

                return;
            }

            try
            {
                if (!await _credentialProvider.IsAuthenticationDisabledAsync().ConfigureAwait(false))
                {
                    var authHeader = httpRequest.Headers.Where(x => x.Key.ToLower() == AuthHeaderName).FirstOrDefault().Value?.FirstOrDefault();
                    var channelId = httpRequest.Headers.Where(x => x.Key.ToLower() == ChannelIdHeaderName).FirstOrDefault().Value?.FirstOrDefault();

                    if (string.IsNullOrWhiteSpace(authHeader))
                    {
                        MissingAuthHeaderHelper(AuthHeaderName, httpResponse);

                        return;
                    }

                    if (string.IsNullOrWhiteSpace(channelId))
                    {
                        MissingAuthHeaderHelper(ChannelIdHeaderName, httpResponse);

                        return;
                    }

                    var claimsIdentity = await JwtTokenValidation.ValidateAuthHeader(authHeader, _credentialProvider, _channelProvider, channelId).ConfigureAwait(false);
                    if (!claimsIdentity.IsAuthenticated)
                    {
                        httpResponse.StatusCode = HttpStatusCode.Unauthorized;

                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                httpResponse.StatusCode = HttpStatusCode.InternalServerError;
                httpResponse.Content = new StringContent("Error while attempting to authorize connection.");

                throw ex;
            }

            CreateStreamingServerConnection(onTurnError, middlewareSet, System.Web.HttpContext.Current, httpResponse, httpRequest, bot);
        }

        private void MissingAuthHeaderHelper(string headerName, HttpResponseMessage httpResponseMessage)
        {
            httpResponseMessage.StatusCode = HttpStatusCode.Unauthorized;
            httpResponseMessage.Content = new StringContent($"Unable to authentiate. Missing header: {headerName}");
        }

        private void CreateStreamingServerConnection(Func<ITurnContext, Exception, Task> onTurnError, List<Builder.IMiddleware> middlewareSet, System.Web.HttpContext httpContext, HttpResponseMessage httpResponse, HttpRequestMessage httpRequest, IBot bot = null)
        {
            var handler = new StreamingRequestHandler(onTurnError, bot ?? httpRequest.GetDependencyScope().GetService(typeof(IBot)) as IBot, middlewareSet);

            Func<AspNetWebSocketContext, Task> processWebSocketSessionFunc = async (context) =>
            {
                IStreamingTransportServer server = new WebSocketServer(context.WebSocket, handler);

                if (server == null)
                {
                    httpResponse.StatusCode = HttpStatusCode.InternalServerError;
                    httpResponse.Content = new StringContent("Unable to create transport server.");

                    return;
                }

                handler.Server = server;
                await server.StartAsync().ConfigureAwait(false);
            };

            // set the status code first , since the next call might have a chance to block the thread.
            httpResponse.StatusCode = HttpStatusCode.SwitchingProtocols;
            httpContext.AcceptWebSocketRequest(processWebSocketSessionFunc);
        }
    }
}
