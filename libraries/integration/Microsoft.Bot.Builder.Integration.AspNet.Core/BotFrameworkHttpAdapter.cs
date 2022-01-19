﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Streaming;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Rest.TransientFaultHandling;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    /// <summary>
    /// A Bot Builder Adapter implementation used to handled bot Framework HTTP requests.
    /// </summary>
    [Obsolete("Use `CloudAdapter` instead.", false)]
    public class BotFrameworkHttpAdapter : BotFrameworkHttpAdapterBase, IBotFrameworkHttpAdapter
    {
        private const string AuthHeaderName = "authorization";
        private const string ChannelIdHeaderName = "channelid";

        /// <summary>
        /// Initializes a new instance of the <see cref="BotFrameworkHttpAdapter"/> class,
        /// using a credential provider.
        /// </summary>
        /// <param name="credentialProvider">The credential provider.</param>
        /// <param name="authConfig">The authentication configuration.</param>
        /// <param name="channelProvider">The channel provider.</param>
        /// <param name="connectorClientRetryPolicy">Retry policy for retrying HTTP operations.</param>
        /// <param name="customHttpClient">The HTTP client.</param>
        /// <param name="middleware">The middleware to initially add to the adapter.</param>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="credentialProvider"/> is <c>null</c>.</exception>
        /// <remarks>Use a <see cref="MiddlewareSet"/> object to add multiple middleware
        /// components in the constructor. Use the IMiddleware method to
        /// add additional middleware to the adapter after construction.
        /// </remarks>
        public BotFrameworkHttpAdapter(
            ICredentialProvider credentialProvider,
            AuthenticationConfiguration authConfig,
            IChannelProvider channelProvider = null,
            RetryPolicy connectorClientRetryPolicy = null,
            HttpClient customHttpClient = null,
            IMiddleware middleware = null,
            ILogger logger = null)
            : base(credentialProvider, authConfig ?? new AuthenticationConfiguration(), channelProvider, connectorClientRetryPolicy, customHttpClient, middleware, logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BotFrameworkHttpAdapter"/> class,
        /// using a credential provider.
        /// </summary>
        /// <param name="credentialProvider">The credential provider.</param>
        /// <param name="channelProvider">The channel provider.</param>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        public BotFrameworkHttpAdapter(ICredentialProvider credentialProvider = null, IChannelProvider channelProvider = null, ILogger<BotFrameworkHttpAdapter> logger = null)
            : this(credentialProvider ?? new SimpleCredentialProvider(), new AuthenticationConfiguration(), channelProvider, null, null, null, logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BotFrameworkHttpAdapter"/> class,
        /// using a credential provider.
        /// </summary>
        /// <param name="credentialProvider">The credential provider.</param>
        /// <param name="channelProvider">The channel provider.</param>
        /// <param name="httpClient">The <see cref="HttpClient"/> used.</param>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        public BotFrameworkHttpAdapter(ICredentialProvider credentialProvider, IChannelProvider channelProvider, HttpClient httpClient, ILogger<BotFrameworkHttpAdapter> logger)
            : this(credentialProvider ?? new SimpleCredentialProvider(), new AuthenticationConfiguration(), channelProvider, null, httpClient, null, logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BotFrameworkHttpAdapter"/> class.
        /// </summary>
        /// <param name="configuration">An <see cref="IConfiguration"/> instance.</param>
        /// <param name="credentialProvider">The credential provider.</param>
        /// <param name="authConfig">The authentication configuration.</param>
        /// <param name="channelProvider">The channel provider.</param>
        /// <param name="connectorClientRetryPolicy">Retry policy for retrying HTTP operations.</param>
        /// <param name="customHttpClient">The HTTP client.</param>
        /// <param name="middleware">The middleware to initially add to the adapter.</param>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        protected BotFrameworkHttpAdapter(
            IConfiguration configuration,
            ICredentialProvider credentialProvider,
            AuthenticationConfiguration authConfig = null,
            IChannelProvider channelProvider = null,
            RetryPolicy connectorClientRetryPolicy = null,
            HttpClient customHttpClient = null,
            IMiddleware middleware = null,
            ILogger logger = null)
            : this(credentialProvider ?? new ConfigurationCredentialProvider(configuration), authConfig ?? new AuthenticationConfiguration(), channelProvider ?? new ConfigurationChannelProvider(configuration), connectorClientRetryPolicy, customHttpClient, middleware, logger)
        {
            var openIdEndpoint = configuration.GetSection(AuthenticationConstants.BotOpenIdMetadataKey)?.Value;

            if (!string.IsNullOrEmpty(openIdEndpoint))
            {
                // Indicate which Cloud we are using, for example, Public or Sovereign.
                ChannelValidation.OpenIdMetadataUrl = openIdEndpoint;
                GovernmentChannelValidation.OpenIdMetadataUrl = openIdEndpoint;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BotFrameworkHttpAdapter"/> class.
        /// </summary>
        /// <param name="configuration">An <see cref="IConfiguration"/> instance.</param>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        protected BotFrameworkHttpAdapter(IConfiguration configuration, ILogger<BotFrameworkHttpAdapter> logger = null)
            : this(configuration, new ConfigurationCredentialProvider(configuration), new AuthenticationConfiguration(), new ConfigurationChannelProvider(configuration), logger: logger)
        {
        }

        /// <summary>
        /// This method can be called from inside a POST method on any Controller implementation.
        /// </summary>
        /// <param name="httpRequest">The HTTP request object, typically in a POST handler by a Controller.</param>
        /// <param name="httpResponse">The HTTP response object.</param>
        /// <param name="bot">The bot implementation.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task ProcessAsync(HttpRequest httpRequest, HttpResponse httpResponse, IBot bot, CancellationToken cancellationToken = default)
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

            if (httpRequest.Method == HttpMethods.Get)
            {
                await ConnectWebSocketAsync(bot, httpRequest, httpResponse, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                // Deserialize the incoming Activity
                var activity = await HttpHelper.ReadRequestAsync<Activity>(httpRequest).ConfigureAwait(false);

                if (string.IsNullOrEmpty(activity?.Type))
                {
                    httpResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                    Logger.LogWarning("BadRequest: Missing activity or activity type.");
                    return;
                }

                // Grab the auth header from the inbound http request
                var authHeader = httpRequest.Headers["Authorization"];

                try
                {
                    // Process the inbound activity with the bot
                    var invokeResponse = await ProcessActivityAsync(authHeader, activity, bot.OnTurnAsync, cancellationToken).ConfigureAwait(false);

                    // write the response, potentially serializing the InvokeResponse
                    await HttpHelper.WriteResponseAsync(httpResponse, invokeResponse).ConfigureAwait(false);
                }
                catch (UnauthorizedAccessException)
                {
                    // handle unauthorized here as this layer creates the http response
                    httpResponse.StatusCode = (int)HttpStatusCode.Unauthorized;
                }
            }
        }

        /// <summary>
        /// Create the <see cref="StreamingRequestHandler"/> for processing for a new Web Socket connection request.
        /// </summary>
        /// <param name="bot">The <see cref="IBot"/> implementation which will process the request.</param>
        /// <param name="socket">The <see cref="WebSocket"/> which the request will be received on.</param>
        /// <param name="audience">The authorized audience of the incoming connection request.</param>
        /// <returns>Returns a new <see cref="StreamingRequestHandler"/> implementation.</returns>
        public virtual StreamingRequestHandler CreateStreamingRequestHandler(IBot bot, WebSocket socket, string audience)
        {
            if (bot == null)
            {
                throw new ArgumentNullException(nameof(bot));
            }

            if (socket == null)
            {
                throw new ArgumentNullException(nameof(socket));
            }

            return new StreamingRequestHandler(bot, this, socket, audience, Logger);
        }

        private static async Task WriteUnauthorizedResponseAsync(string headerName, HttpRequest httpRequest)
        {
            httpRequest.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await httpRequest.HttpContext.Response.WriteAsync($"Unable to authenticate. Missing header: {headerName}").ConfigureAwait(false);
        }

        /// <summary>
        /// Process the initial request to establish a long lived connection via a streaming server.
        /// </summary>
        /// <param name="bot">The <see cref="IBot"/> instance.</param>
        /// <param name="httpRequest">The connection request.</param>
        /// <param name="httpResponse">The response sent on error or connection termination.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Returns on task completion.</returns>
        private async Task ConnectWebSocketAsync(IBot bot, HttpRequest httpRequest, HttpResponse httpResponse, CancellationToken cancellationToken = default)
        {
            if (httpRequest == null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }

            if (httpResponse == null)
            {
                throw new ArgumentNullException(nameof(httpResponse));
            }

            ConnectedBot = bot ?? throw new ArgumentNullException(nameof(bot));

            if (!httpRequest.HttpContext.WebSockets.IsWebSocketRequest)
            {
                httpRequest.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await httpRequest.HttpContext.Response.WriteAsync("Upgrade to WebSocket is required.").ConfigureAwait(false);

                return;
            }

            var claimsIdentity = await AuthenticateRequestAsync(httpRequest).ConfigureAwait(false);
            if (claimsIdentity == null)
            {
                httpRequest.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await httpRequest.HttpContext.Response.WriteAsync("Request authentication failed.").ConfigureAwait(false);

                return;
            }

            try
            {
                // Set ClaimsIdentity on Adapter to enable Skills and User OAuth in WebSocket-based streaming scenarios.
                var audience = GetAudience(claimsIdentity);

                var socket = await httpRequest.HttpContext.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
                var requestHandler = CreateStreamingRequestHandler(bot, socket, audience);

                if (RequestHandlers == null)
                {
                    RequestHandlers = new List<StreamingRequestHandler>();
                }

                RequestHandlers.Add(requestHandler);

                Log.WebSocketConnectionStarted(Logger);
                await requestHandler.ListenAsync(cancellationToken).ConfigureAwait(false);
                Log.WebSocketConnectionCompleted(Logger);
            }
            catch (Exception ex)
            {
                httpRequest.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await httpRequest.HttpContext.Response.WriteAsync($"Unable to create transport server. Error: {ex.ToString()}").ConfigureAwait(false);

                throw;
            }
        }

        /// <summary>
        /// Validates the auth header for WebSocket upgrade requests.
        /// </summary>
        /// <remarks>
        /// Returns a ClaimsIdentity for successful auth and when auth is disabled. Returns null for failed auth.
        /// </remarks>
        /// <param name="httpRequest">The connection request.</param>
        private async Task<ClaimsIdentity> AuthenticateRequestAsync(HttpRequest httpRequest)
        {
            try
            {
                if (!await CredentialProvider.IsAuthenticationDisabledAsync().ConfigureAwait(false))
                {
                    var authHeader = httpRequest.Headers.First(x => string.Equals(x.Key, AuthHeaderName, StringComparison.OrdinalIgnoreCase)).Value.FirstOrDefault();
                    var channelId = httpRequest.Headers.First(x => string.Equals(x.Key, ChannelIdHeaderName, StringComparison.OrdinalIgnoreCase)).Value.FirstOrDefault();

                    if (string.IsNullOrWhiteSpace(authHeader))
                    {
                        await WriteUnauthorizedResponseAsync(AuthHeaderName, httpRequest).ConfigureAwait(false);
                        return null;
                    }

                    if (string.IsNullOrWhiteSpace(channelId))
                    {
                        await WriteUnauthorizedResponseAsync(ChannelIdHeaderName, httpRequest).ConfigureAwait(false);
                        return null;
                    }

                    var claimsIdentity = await JwtTokenValidation.ValidateAuthHeader(authHeader, CredentialProvider, ChannelProvider, channelId).ConfigureAwait(false);
                    if (!claimsIdentity.IsAuthenticated)
                    {
                        httpRequest.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        return null;
                    }
                    
                    ClaimsIdentity = claimsIdentity;
                    return claimsIdentity;
                }

                // Authentication is not enabled, therefore return an anonymous ClaimsIdentity.
                return new ClaimsIdentity(new List<Claim>(), "anonymous");
            }
            catch (Exception)
            {
                httpRequest.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await httpRequest.HttpContext.Response.WriteAsync("Error while attempting to authorize connection.").ConfigureAwait(false);

                throw;
            }
        }

        /// <summary>
        /// Get the audience for the WebSocket connection from the authenticated ClaimsIdentity.
        /// </summary>
        /// <remarks>
        /// Setting the Audience on the StreamingRequestHandler enables the bot to call skills and correctly forward responses from the skill to the next recipient.
        /// i.e. the participant at the other end of the WebSocket connection.
        /// </remarks>
        /// <param name="claimsIdentity">ClaimsIdentity for authenticated caller.</param>
        private string GetAudience(ClaimsIdentity claimsIdentity)
        {
            if (claimsIdentity.AuthenticationType != AuthenticationConstants.AnonymousAuthType)
            {
                var audience = ChannelProvider != null && ChannelProvider.IsGovernment() ?
    GovernmentAuthenticationConstants.ToChannelFromBotOAuthScope :
    AuthenticationConstants.ToChannelFromBotOAuthScope;

                if (SkillValidation.IsSkillClaim(claimsIdentity.Claims))
                {
                    audience = JwtTokenValidation.GetAppIdFromClaims(claimsIdentity.Claims);
                }

                return audience;
            }

            return null;
        }

        private class Log
        {
            private static readonly Action<ILogger, Exception> _webSocketConnectionStarted =
                LoggerMessage.Define(LogLevel.Information, new EventId(1, nameof(WebSocketConnectionStarted)), "WebSocket connection started.");

            private static readonly Action<ILogger, Exception> _webSocketConnectionCompleted =
                LoggerMessage.Define(LogLevel.Information, new EventId(2, nameof(WebSocketConnectionCompleted)), "WebSocket connection completed.");

            public static void WebSocketConnectionStarted(ILogger logger) => _webSocketConnectionStarted(logger, null);

            public static void WebSocketConnectionCompleted(ILogger logger) => _webSocketConnectionCompleted(logger, null);
        }
    }
}
