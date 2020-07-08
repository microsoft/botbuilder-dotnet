// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
                await ConnectWebSocketAsync(bot, httpRequest, httpResponse).ConfigureAwait(false);
            }
            else
            {
                // Deserialize the incoming Activity
                var activity = await HttpHelper.ReadRequestAsync<Activity>(httpRequest).ConfigureAwait(false);

                if (string.IsNullOrEmpty(activity?.Type))
                {
                    httpResponse.StatusCode = (int)HttpStatusCode.BadRequest;
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
        /// <returns>Returns on task completion.</returns>
        private async Task ConnectWebSocketAsync(IBot bot, HttpRequest httpRequest, HttpResponse httpResponse)
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

            if (!await AuthenticateRequestAsync(httpRequest).ConfigureAwait(false))
            {
                httpRequest.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await httpRequest.HttpContext.Response.WriteAsync("Request authentication failed.").ConfigureAwait(false);

                return;
            }

            try
            {
                var socket = await httpRequest.HttpContext.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
                
                var requestHandler = new StreamingRequestHandler(bot, this, socket, Logger);

                if (RequestHandlers == null)
                {
                    RequestHandlers = new List<StreamingRequestHandler>();
                }

                RequestHandlers.Add(requestHandler);

                await requestHandler.ListenAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                httpRequest.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await httpRequest.HttpContext.Response.WriteAsync($"Unable to create transport server. Error: {ex.ToString()}").ConfigureAwait(false);

                throw;
            }
        }

        private async Task<bool> AuthenticateRequestAsync(HttpRequest httpRequest)
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
                        return false;
                    }

                    if (string.IsNullOrWhiteSpace(channelId))
                    {
                        await WriteUnauthorizedResponseAsync(ChannelIdHeaderName, httpRequest).ConfigureAwait(false);
                        return false;
                    }

                    var claimsIdentity = await JwtTokenValidation.ValidateAuthHeader(authHeader, CredentialProvider, ChannelProvider, channelId).ConfigureAwait(false);
                    if (!claimsIdentity.IsAuthenticated)
                    {
                        httpRequest.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        return false;
                    }
                    
                    // Add ServiceURL to the cache of trusted sites in order to allow token refreshing.
                    AppCredentials.TrustServiceUrl(claimsIdentity.FindFirst(AuthenticationConstants.ServiceUrlClaim).Value);
                    ClaimsIdentity = claimsIdentity;
                }

                return true;
            }
            catch (Exception)
            {
                httpRequest.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await httpRequest.HttpContext.Response.WriteAsync("Error while attempting to authorize connection.").ConfigureAwait(false);

                throw;
            }
        }
    }
}
