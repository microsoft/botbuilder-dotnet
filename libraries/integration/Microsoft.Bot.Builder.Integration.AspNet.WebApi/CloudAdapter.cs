﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.WebSockets;
using Microsoft.Bot.Builder.Streaming;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Connector.Streaming.Application;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Streaming;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder.Integration.AspNet.WebApi
{
    /// <summary>
    /// An adapter that implements the Bot Framework Protocol and can be hosted in different cloud environmens both public and private.
    /// </summary>
    public class CloudAdapter : CloudAdapterBase, IBotFrameworkHttpAdapter
    {
        private const string AuthHeaderName = "authorization";
        private const string ChannelIdHeaderName = "channelid";
        private readonly ConcurrentDictionary<Guid, StreamingActivityProcessor> _streamingConnections = new ConcurrentDictionary<Guid, StreamingActivityProcessor>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudAdapter"/> class. (Public cloud. No auth. For testing.)
        /// </summary>
        public CloudAdapter()
            : this(BotFrameworkAuthenticationFactory.Create())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudAdapter"/> class.
        /// </summary>
        /// <param name="botFrameworkAuthentication">The <see cref="BotFrameworkAuthentication"/> this adapter should use.</param>
        /// <param name="logger">The <see cref="ILogger"/> implementation this adapter should use.</param>
        public CloudAdapter(
            BotFrameworkAuthentication botFrameworkAuthentication,
            ILogger logger = null)
            : base(botFrameworkAuthentication, logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudAdapter"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="IConfiguration"/> instance.</param>
        /// <param name="httpClientFactory">The <see cref="IHttpClientFactory"/> this adapter should use.</param>
        /// <param name="logger">The <see cref="ILogger"/> implementation this adapter should use.</param>
        public CloudAdapter(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory = null,
            ILogger logger = null)
            : this(new ConfigurationBotFrameworkAuthentication(configuration, httpClientFactory: httpClientFactory, logger: logger), logger)
        {
        }

        /// <inheritdoc/>
        public async Task ProcessAsync(HttpRequestMessage httpRequest, HttpResponseMessage httpResponse, IBot bot, CancellationToken cancellationToken = default)
        {
            _ = httpRequest ?? throw new ArgumentNullException(nameof(httpRequest));
            _ = httpResponse ?? throw new ArgumentNullException(nameof(httpResponse));
            _ = bot ?? throw new ArgumentNullException(nameof(bot));

            try
            {
                HttpContextWrapper httpContext = null;
                httpRequest.Properties.TryGetValue("MS_HttpContext", out var value);
                if (value != null)
                {
                    httpContext = value as HttpContextWrapper;
                }
                else if (HttpContext.Current != null)
                {
                    httpContext = new HttpContextWrapper(HttpContext.Current);
                }

                // Only GET requests for web socket connects are allowed
                if (httpRequest.Method == HttpMethod.Get && httpContext?.IsWebSocketRequest == true)
                {
                    httpContext.AcceptWebSocketRequest(async context =>
                    {
                        // All socket communication will be handled by the internal streaming-specific BotAdapter
                        await ConnectAsync(context, httpRequest, bot, cancellationToken).ConfigureAwait(false);
                    });
                    httpResponse.StatusCode = HttpStatusCode.SwitchingProtocols;
                }
                else if (httpRequest.Method == HttpMethod.Post)
                {
                    // deserialize the incoming Activity
                    var activity = await HttpHelper.ReadRequestAsync(httpRequest, cancellationToken).ConfigureAwait(false);

                    if (string.IsNullOrEmpty(activity?.Type))
                    {
                        httpResponse.StatusCode = HttpStatusCode.BadRequest;
                        Logger.LogWarning("BadRequest: Missing activity or activity type.");
                        return;
                    }

                    // grab the auth header from the inbound http request
                    var authHeader = httpRequest.Headers.Authorization?.ToString();

                    // process the inbound activity with the bot
                    var invokeResponse = await ProcessActivityAsync(authHeader, activity, bot.OnTurnAsync, cancellationToken).ConfigureAwait(false);

                    // write the response, potentially serializing the InvokeResponse
                    HttpHelper.WriteResponse(httpRequest, httpResponse, invokeResponse);
                }
                else
                {
                    httpResponse.StatusCode = HttpStatusCode.MethodNotAllowed;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                // handle unauthorized here as this layer creates the http response
                httpResponse.StatusCode = HttpStatusCode.Unauthorized;

                Logger.LogError(ex.ToString());
            }
        }

        /// <summary>
        /// Used to connect the adapter to a named pipe.
        /// </summary>
        /// <param name="pipeName">The name of the named pipe.</param>
        /// <param name="bot">The bot instance to use.</param>
        /// <param name="appId">The bot's application id.</param>
        /// <param name="audience">The audience to use for outbound communication. This will vary by cloud environment.</param>
        /// <param name="callerId">The callerId, this may be NULL.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>
        public async Task ConnectNamedPipeAsync(string pipeName, IBot bot, string appId, string audience, string callerId)
        {
            if (string.IsNullOrEmpty(pipeName))
            {
                throw new ArgumentNullException(nameof(pipeName));
            }

            _ = bot ?? throw new ArgumentNullException(nameof(bot));

            if (string.IsNullOrEmpty(audience))
            {
                throw new ArgumentNullException(nameof(audience));
            }

            // The named pipe is local and so there is no network authentication to perform: so we can create the result here.
            var authenticationRequestResult = new AuthenticateRequestResult
            {
                Audience = audience,
                ClaimsIdentity = appId != null ? CreateClaimsIdentity(appId) : new ClaimsIdentity(),
                CallerId = callerId
            };

            // Tie the authentication results, the named pipe, the adapter and the bot together to be ready to handle any inbound activities
            var connectionId = Guid.NewGuid();
            using (var scope = Logger.BeginScope(connectionId))
            {
                while (true)
                {
#pragma warning disable CA2000 // Dispose objects before losing scope: StreamingRequestHandler is responsible for disposing StreamingConnection
                    var connection = new NamedPipeStreamingConnection(pipeName, Logger);
#pragma warning restore CA2000 // Dispose objects before losing scope

                    using (var streamingActivityProcessor = new StreamingActivityProcessor(authenticationRequestResult, connection, this, bot))
                    {
                        // Start receiving activities on the named pipe
                        _streamingConnections.TryAdd(connectionId, streamingActivityProcessor);
                        Log.NamedPipeConnectionStarted(Logger);
                        await streamingActivityProcessor.ListenAsync(CancellationToken.None).ConfigureAwait(false);
                        _streamingConnections.TryRemove(connectionId, out _);
                        Log.NamedPipeConnectionCompleted(Logger);
                        Logger.LogWarning("Named pipe got disconnected. Reconnecting.");
                    }
                }
            }
        }

        /// <inheritdoc/>
        protected override ConnectorFactory GetStreamingConnectorFactory(Activity activity)
        {
            foreach (var connection in _streamingConnections.Values)
            {
                if (connection.HandlesActivity(activity))
                {
                    return connection.GetConnectorFactory();
                }
            }

            throw new ApplicationException($"No streaming connection found for activity: {activity}");
        }

        /// <summary>
        /// Creates a <see cref="StreamingConnection"/> that uses web sockets.
        /// </summary>
        /// <param name="socket"><see cref="WebSocket"/> instance on which streams are transported between client and server.</param>
        /// <param name="logger">Logger implementation for tracing and debugging information.</param>
        /// <returns><see cref="StreamingConnection"/> that uses web socket.</returns>
        protected virtual StreamingConnection CreateWebSocketConnection(WebSocket socket, ILogger logger)
        {
            if (socket == null)
            {
                throw new ArgumentNullException(nameof(socket));
            }

            return new WebSocketStreamingConnection(socket, logger);
        }

        private async Task ConnectAsync(AspNetWebSocketContext context, HttpRequestMessage httpRequest, IBot bot, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Received request for web socket connect.");

            // Grab the auth header from the inbound http request
            var authHeader = httpRequest.Headers.GetValues(AuthHeaderName.ToLowerInvariant()).FirstOrDefault();

            // Grab the channelId which should be in the http headers
            var channelIdHeader = httpRequest.Headers.GetValues(ChannelIdHeaderName.ToLowerInvariant()).FirstOrDefault();

            var authenticationRequestResult = await BotFrameworkAuthentication.AuthenticateStreamingRequestAsync(authHeader, channelIdHeader, cancellationToken).ConfigureAwait(false);

            var connectionId = Guid.NewGuid();
            using (var scope = Logger.BeginScope(connectionId))
            {
                var connection = CreateWebSocketConnection(context.WebSocket, Logger);

                using (var streamingActivityProcessor = new StreamingActivityProcessor(authenticationRequestResult, connection, this, bot))
                {
                    // Start receiving activities on the socket
                    _streamingConnections.TryAdd(connectionId, streamingActivityProcessor);
                    Log.WebSocketConnectionStarted(Logger);
                    await streamingActivityProcessor.ListenAsync(cancellationToken).ConfigureAwait(false);
                    _streamingConnections.TryRemove(connectionId, out _);
                    Log.WebSocketConnectionCompleted(Logger);
                }
            }
        }

        private class StreamingActivityProcessor : IStreamingActivityProcessor, IDisposable
        {
            private readonly AuthenticateRequestResult _authenticateRequestResult;
            private readonly CloudAdapter _adapter;
            private readonly StreamingRequestHandler _requestHandler;

            public StreamingActivityProcessor(AuthenticateRequestResult authenticateRequestResult, StreamingConnection connection, CloudAdapter adapter, IBot bot)
            {
                _authenticateRequestResult = authenticateRequestResult;
                _adapter = adapter;

                // Internal reuse of the existing StreamingRequestHandler class
                _requestHandler = new StreamingRequestHandler(bot, this, connection, authenticateRequestResult.Audience, logger: adapter.Logger);

                // Fix up the connector factory so connector create from it will send over this connection
                _authenticateRequestResult.ConnectorFactory = new StreamingConnectorFactory(_requestHandler);
            }

            public StreamingActivityProcessor(AuthenticateRequestResult authenticateRequestResult, string pipeName, CloudAdapter adapter, IBot bot)
            {
                _authenticateRequestResult = authenticateRequestResult;
                _adapter = adapter;

                // Internal reuse of the existing StreamingRequestHandler class
                _requestHandler = new StreamingRequestHandler(bot, this, pipeName, _authenticateRequestResult.Audience, adapter.Logger);

                // Fix up the connector factory so connector create from it will send over this connection
                _authenticateRequestResult.ConnectorFactory = new StreamingConnectorFactory(_requestHandler);
            }

            public bool HandlesActivity(Activity activity)
            {
                if (string.IsNullOrWhiteSpace(_requestHandler.ServiceUrl))
                {
                    return false;
                }

                if (string.IsNullOrWhiteSpace(activity.ServiceUrl))
                {
                    return false;
                }

                return _requestHandler.ServiceUrl.Equals(activity.ServiceUrl, StringComparison.OrdinalIgnoreCase) &&
                       _requestHandler.HasConversation(activity.Conversation.Id);
            }

            public ConnectorFactory GetConnectorFactory()
            {
                return _authenticateRequestResult.ConnectorFactory;
            }

            public void Dispose()
            {
                _requestHandler?.Dispose();
            }

            public Task ListenAsync(CancellationToken cancellationToken) => _requestHandler.ListenAsync(cancellationToken);

            Task<InvokeResponse> IStreamingActivityProcessor.ProcessStreamingActivityAsync(Activity activity, BotCallbackHandler callback, CancellationToken cancellationToken)
                => _adapter.ProcessActivityAsync(_authenticateRequestResult, activity, callback, cancellationToken);

            private class StreamingConnectorFactory : ConnectorFactory
            {
                private readonly StreamingRequestHandler _requestHandler;
                private string _serviceUrl = null;

                public StreamingConnectorFactory(StreamingRequestHandler requestHandler)
                {
                    _requestHandler = requestHandler;
                }

                public override Task<IConnectorClient> CreateAsync(string serviceUrl, string audience, CancellationToken cancellationToken)
                {
                    if (_serviceUrl == null)
                    {
                        _serviceUrl = serviceUrl;
                    }
                    else if (!_serviceUrl.Equals(serviceUrl, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new NotSupportedException("This is a streaming scenario, all connectors from this factory must all be for the same url.");
                    }

#pragma warning disable CA2000 // Dispose objects before losing scope
                    var streamingHttpClient = new StreamingHttpClient(_requestHandler);
#pragma warning restore CA2000 // Dispose objects before losing scope

                    return Task.FromResult<IConnectorClient>(new ConnectorClient(MicrosoftAppCredentials.Empty, streamingHttpClient, false));
                }

                private class StreamingHttpClient : HttpClient
                {
                    private readonly StreamingRequestHandler _requestHandler;

                    public StreamingHttpClient(StreamingRequestHandler requestHandler)
                    {
                        _requestHandler = requestHandler ?? throw new ArgumentNullException(nameof(requestHandler));
                    }

                    public override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken)
                    {
                        var streamingRequest = await CreateSteamingRequestAsync(httpRequestMessage).ConfigureAwait(false);
                        var receiveResponse = await _requestHandler.SendStreamingRequestAsync(streamingRequest, cancellationToken).ConfigureAwait(false);
                        var httpResponseMessage = await CreateHttpResponseAsync(receiveResponse).ConfigureAwait(false);
                        return httpResponseMessage;
                    }

                    private async Task<StreamingRequest> CreateSteamingRequestAsync(HttpRequestMessage httpRequestMessage)
                    {
                        var streamingRequest = new StreamingRequest
                        {
                            Path = httpRequestMessage.RequestUri.OriginalString.Substring(httpRequestMessage.RequestUri.OriginalString.IndexOf("/v3", StringComparison.Ordinal)),
                            Verb = httpRequestMessage.Method.ToString(),
                        };

                        if (httpRequestMessage.Content != null)
                        {
                            streamingRequest.SetBody(await httpRequestMessage.Content.ReadAsStringAsync().ConfigureAwait(false));
                        }

                        return streamingRequest;
                    }

                    private async Task<HttpResponseMessage> CreateHttpResponseAsync(ReceiveResponse receiveResponse)
                    {
                        var httpResponseMessage = new HttpResponseMessage((HttpStatusCode)receiveResponse.StatusCode);
                        httpResponseMessage.Content = new StringContent(await receiveResponse.ReadBodyAsStringAsync().ConfigureAwait(false));
                        return httpResponseMessage;
                    }
                }
            }
        }

        private class Log
        {
            private static readonly Action<ILogger, Exception> _namedPipeConnectionStarted =
                LoggerMessage.Define(LogLevel.Information, new EventId(1, nameof(NamedPipeConnectionStarted)), "NamedPipe connection started.");

            private static readonly Action<ILogger, Exception> _namedConnectionCompleted =
                LoggerMessage.Define(LogLevel.Information, new EventId(2, nameof(NamedPipeConnectionCompleted)), "NamedPipe connection completed.");

            private static readonly Action<ILogger, Exception> _webSocketConnectionStarted =
                LoggerMessage.Define(LogLevel.Information, new EventId(1, nameof(WebSocketConnectionStarted)), "WebSocket connection started.");

            private static readonly Action<ILogger, Exception> _webSocketConnectionCompleted =
                LoggerMessage.Define(LogLevel.Information, new EventId(2, nameof(WebSocketConnectionCompleted)), "WebSocket connection completed.");

            public static void NamedPipeConnectionStarted(ILogger logger) => _namedPipeConnectionStarted(logger, null);

            public static void NamedPipeConnectionCompleted(ILogger logger) => _namedConnectionCompleted(logger, null);

            public static void WebSocketConnectionStarted(ILogger logger) => _webSocketConnectionStarted(logger, null);

            public static void WebSocketConnectionCompleted(ILogger logger) => _webSocketConnectionCompleted(logger, null);
        }
    }
}
