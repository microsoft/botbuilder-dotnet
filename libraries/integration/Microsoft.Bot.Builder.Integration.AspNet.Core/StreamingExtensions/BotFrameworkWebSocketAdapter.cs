// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Protocol.WebSockets;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.StreamingExtensions
{
    public class BotFrameworkWebSocketAdapter : IBotFrameworkHttpAdapter
    {
        private readonly IChannelProvider _channelProvider;
        private readonly ICredentialProvider _credentialProvider;
        

        public BotFrameworkWebSocketAdapter(ICredentialProvider credentialProvider, IChannelProvider channelProvider = null, ILogger<BotFrameworkWebSocketAdapter> logger = null)
        {
            this._credentialProvider = credentialProvider;
            this._channelProvider = channelProvider;
        }

        public ConcurrentDictionary<string, WebSocketServer> Connections { get; set; }

        public async Task ProcessAsync(HttpRequest httpRequest, HttpResponse httpResponse, IBot bot, CancellationToken cancellationToken = default(CancellationToken))
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

            var authHeader = httpRequest.Headers["Authorization"];
            var channelId = httpRequest.Headers["ChannelId"];
            try
            {
                var claimsIdentity = await JwtTokenValidation.ValidateAuthHeader(authHeader, _credentialProvider, _channelProvider, channelId).ConfigureAwait(false);
                if (!claimsIdentity.IsAuthenticated)
                {
                    httpRequest.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return;
                }
            }
            catch (Exception)
            {
                httpRequest.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return;
            }

            if (!httpRequest.HttpContext.WebSockets.IsWebSocketRequest)
            {
                httpRequest.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await httpRequest.HttpContext.Response.WriteAsync("Upgrade to WebSocket required.").ConfigureAwait(false);
                return;
            }

            await CreateWebSocketConnectionAsync(httpRequest.HttpContext, authHeader, channelId, bot).ConfigureAwait(false);
        }

        public async Task CreateWebSocketConnectionAsync(HttpContext httpContext, string authHeader, string channelId, IBot bot)
        {
            var socket = await httpContext.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
            var serverID = Guid.NewGuid().ToString();
            var server = new WebSocketServer(socket, new StreamingExtensionRequestHandler(new BotFrameworkStreamingExtensionsAdapter(serverID), bot));

            try
            {
                WebSocketServerRegistry.RegisterNewServer(serverID, server);
            }
            catch (Exception)
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return;
            }

            var startListening = server.StartAsync();
            Task.WaitAll(startListening);
        }
    }
}
