// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Streaming;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Streaming;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
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

        /// <summary>
        /// Process the inbound HTTP request with the bot resulting in the outbound http response, this method can be called directly from a Controller.
        /// If the HTTP method is a POST the body will contain the <see cref="Activity"/> to process. 
        /// </summary>
        /// <param name="httpRequest">The <see cref="HttpRequest"/>.</param>
        /// <param name="httpResponse">The <see cref="HttpResponse"/>.</param>
        /// <param name="bot">The <see cref="IBot"/> implementation to use for this request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>
        public async Task ProcessAsync(HttpRequest httpRequest, HttpResponse httpResponse, IBot bot, CancellationToken cancellationToken = default)
        {
            _ = httpRequest ?? throw new ArgumentNullException(nameof(httpRequest));
            _ = httpResponse ?? throw new ArgumentNullException(nameof(httpResponse));
            _ = bot ?? throw new ArgumentNullException(nameof(bot));

            try
            {
                // Only GET requests for web socket connects are allowed
                if (httpRequest.Method == HttpMethods.Get && httpRequest.HttpContext.WebSockets.IsWebSocketRequest)
                {
                    // All socket communication will be handled by the internal streaming-specific BotAdapter
                    await ConnectAsync(httpRequest, bot, cancellationToken).ConfigureAwait(false);
                }
                else if (httpRequest.Method == HttpMethods.Post)
                {
                    // Deserialize the incoming Activity
                    var activity = await HttpHelper.ReadRequestAsync<Activity>(httpRequest).ConfigureAwait(false);

                    // A POST request must contain an Activity 
                    if (string.IsNullOrEmpty(activity?.Type))
                    {
                        httpResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                        return;
                    }

                    // Grab the auth header from the inbound http request
                    var authHeader = httpRequest.Headers["Authorization"];

                    // Process the inbound activity with the bot
                    var invokeResponse = await ProcessActivityAsync(authHeader, activity, bot.OnTurnAsync, cancellationToken).ConfigureAwait(false);

                    // Write the response, potentially serializing the InvokeResponse
                    await HttpHelper.WriteResponseAsync(httpResponse, invokeResponse).ConfigureAwait(false);
                }
                else
                {
                    httpResponse.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                }
            }
            catch (UnauthorizedAccessException)
            {
                // handle unauthorized here as this layer creates the http response
                httpResponse.StatusCode = (int)HttpStatusCode.Unauthorized;
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
            var streamingActivityProcessor = new StreamingActivityProcessor(authenticationRequestResult, pipeName, this, bot);

            // Start receiving activities on the named pipe
            await streamingActivityProcessor.ListenAsync().ConfigureAwait(false);
        }

        private async Task ConnectAsync(HttpRequest httpRequest, IBot bot, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Received request for web socket connect.");

            // Grab the auth header from the inbound http request
            var authHeader = httpRequest.Headers["Authorization"];

            // Grab the channelId which should be in the http headers
            var channelIdHeader = httpRequest.Headers["channelid"];

            var authenticationRequestResult = await BotFrameworkAuthentication.AuthenticateStreamingRequestAsync(authHeader, channelIdHeader, cancellationToken).ConfigureAwait(false);

            // Transition the request to a WebSocket connection
            var socket = await httpRequest.HttpContext.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);

            // Tie the authentication results, the socket, the adapter and the bot together to be ready to handle any inbound activities
            var streamingActivityProcessor = new StreamingActivityProcessor(authenticationRequestResult, socket, this, bot);

            // Start receiving activities on the socket
            await streamingActivityProcessor.ListenAsync().ConfigureAwait(false);
        }

        private class StreamingActivityProcessor : IStreamingActivityProcessor
        {
            private readonly AuthenticateRequestResult _authenticateRequestResult;
            private readonly CloudAdapter _adapter;
            private readonly StreamingRequestHandler _requestHandler;

            public StreamingActivityProcessor(AuthenticateRequestResult authenticateRequestResult, WebSocket socket, CloudAdapter adapter, IBot bot)
            {
                _authenticateRequestResult = authenticateRequestResult;
                _adapter = adapter;

                // Internal reuse of the existing StreamingRequestHandler class
                _requestHandler = new StreamingRequestHandler(bot, this, socket, _authenticateRequestResult.Audience, adapter.Logger);

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

            public Task ListenAsync() => _requestHandler.ListenAsync();

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
                        streamingRequest.SetBody(await httpRequestMessage.Content.ReadAsStringAsync().ConfigureAwait(false));
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
    }
}
