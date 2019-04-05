// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Protocol.WebSockets;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder.Integration.AspNet.WebApi.StreamingExtensions
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

        public async Task ProcessAsync(HttpRequestMessage httpRequestMessage, HttpResponseMessage httpResponseMessage, IBot bot, CancellationToken cancellationToken = default(CancellationToken))
         {
            var requestContext = httpRequestMessage.GetRequestContext();

            if (httpRequestMessage == null)
            {
                throw new ArgumentNullException(nameof(httpRequestMessage));
            }

            if (httpResponseMessage == null)
            {
                throw new ArgumentNullException(nameof(httpResponseMessage));
            }

            if (bot == null)
            {
                throw new ArgumentNullException(nameof(bot));
            }

            var context = System.Web.HttpContext.Current;
            if (!context.IsWebSocketRequest && !context.IsWebSocketRequestUpgrading)
            {
                httpRequestMessage.CreateErrorResponse(HttpStatusCode.BadRequest, "Upgrade to WebSocket required.");
                return;
            }

            try
            {
                var authHeader = httpRequestMessage.Headers.Where(x => x.Key.ToLower() == "authorization").FirstOrDefault().Value;
                var channelId = httpRequestMessage.Headers.Where(x => x.Key.ToLower() == "channelid").FirstOrDefault().Value;
                var claimsIdentity = await JwtTokenValidation.ValidateAuthHeader(authHeader.FirstOrDefault(), _credentialProvider, _channelProvider, channelId.FirstOrDefault()).ConfigureAwait(false);
                if (!claimsIdentity.IsAuthenticated)
                {
                    httpRequestMessage.CreateErrorResponse(HttpStatusCode.Unauthorized, "Unauthorized.");
                    return;
                }

                await CreateWebSocketConnectionAsync(context, bot).ConfigureAwait(false);
            }
            catch (Exception)
            {
                httpRequestMessage.CreateErrorResponse(HttpStatusCode.Unauthorized, "Unauthorized.");
                return;
            }
        }

        public async Task CreateWebSocketConnectionAsync(System.Web.HttpContext httpContext, IBot bot)
        {
            var socket = new System.Net.WebSockets.ClientWebSocket();
            httpContext.DisposeOnPipelineCompleted(socket);
            var handler = new StreamingExtensionsRequestHandler();
            var server = new WebSocketServer(socket, handler);
            handler.Server = server;
            handler.Bot = bot;
            var startListening = server.StartAsync();
            await startListening.ConfigureAwait(false);
        }
    }
}
