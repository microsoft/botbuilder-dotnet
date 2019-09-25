// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.Bot.Builder.StreamingExtensions
{
    /// <summary>
    /// A Bot Builder Adapter implementation used to handle Bot Framework HTTP and streaming requests. Supports the Bot Framework Protocol v3 with Streaming Extensions.
    /// </summary>
    public class DirectLineAdapter : BotFrameworkAdapter, IBotFrameworkHttpAdapter
    {
        private const string AuthHeaderName = "authorization";
        private const string ChannelIdHeaderName = "channelid";
        private const string InvokeResponseKey = "DirectLineAdapter.InvokeResponse";
        private const string BotIdentityKey = "BotIdentity";
        private readonly IChannelProvider _channelProvider;
        private readonly ICredentialProvider _credentialProvider;
        private readonly ILogger _logger;
        private IBot _bot;
        private ClaimsIdentity _claimsIdentity;
        private IList<StreamingRequestHandler> _requestHandlers;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectLineAdapter"/> class for processing HTTP or streaming requests.
        /// </summary>
        /// <param name="credentialProvider">Optional credential provider to use for authorization.</param>
        /// <param name="channelProvider">Optional channel provider for use with authorization.</param>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        public DirectLineAdapter(ICredentialProvider credentialProvider = null, IChannelProvider channelProvider = null, ILogger<BotFrameworkHttpAdapter> logger = null)
            : base(credentialProvider ?? new SimpleCredentialProvider(), channelProvider, null, null, null, logger)
        {
            _credentialProvider = credentialProvider;
            _channelProvider = channelProvider;
            if (logger != null)
            {
                _logger = logger;
            }
            else
            {
                _logger = NullLogger.Instance;
            }
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="DirectLineAdapter"/> class for processing HTTP requests.
        /// </summary>
        /// <param name="credentialProvider">The credential provider to use for authorization.</param>
        /// <param name="channelProvider">The channel provider for use with authorization.</param>
        /// <param name="httpClient">The HTTP client to use when sending messages to the channel, services, and skills.</param>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        public DirectLineAdapter(ICredentialProvider credentialProvider, IChannelProvider channelProvider, HttpClient httpClient, ILogger<BotFrameworkHttpAdapter> logger)
            : base(credentialProvider ?? new SimpleCredentialProvider(), channelProvider, null, httpClient, null, logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectLineAdapter"/> class for processing streaming requests.
        /// The StreamingRequestHandler serves as a translation layer between the transport layer and bot adapter.
        /// It receives ReceiveRequests from the transport and provides them to the bot adapter in a form
        /// it is able to build activities out of, which are then handed to the bot itself to processed.
        /// Throws <see cref="ArgumentNullException"/> if arguments are null.
        /// </summary>
        /// <param name="onTurnError">Optional function to perform on turn errors.</param>
        /// <param name="bot">The <see cref="IBot"/> to be used for all requests to this handler.</param>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        public DirectLineAdapter(Func<ITurnContext, Exception, Task> onTurnError, IBot bot, ILogger<BotFrameworkHttpAdapter> logger = null)
            : base(new SimpleCredentialProvider())
        {
            OnTurnError = onTurnError;
            _bot = bot ?? throw new ArgumentNullException(nameof(bot));
            if (logger != null)
            {
                _logger = logger;
            }
            else
            {
                _logger = NullLogger.Instance;
            }
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="DirectLineAdapter"/> class for processing HTTP requests.
        /// </summary>
        /// <param name="configuration"> The configuration containing credential and channel provider details for this adapter. </param>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        protected DirectLineAdapter(IConfiguration configuration, ILogger<BotFrameworkHttpAdapter> logger = null)
             : base(new ConfigurationCredentialProvider(configuration), new ConfigurationChannelProvider(configuration), customHttpClient: null, middleware: null, logger: logger)
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
        /// Initial entry point from the bot controller. Validates request and invokes a response from the bot.
        /// Also detects and handles WebSocket upgrade requests in the case of streaming connections.
        /// </summary>
        /// <param name="httpRequest">The request to process.</param>
        /// <param name="httpResponse">The response to return to the client.</param>
        /// <param name="bot">The bot to use when processing the request.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task signifying if the work has been completed.</returns>
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

            _bot = bot ?? throw new ArgumentNullException(nameof(bot));

            if (httpRequest.Method == HttpMethods.Get)
            {
                await ConnectWebSocket(httpRequest, httpResponse).ConfigureAwait(false);
            }
            else
            {
                // deserialize the incoming Activity
                var activity = HttpHelper.ReadRequest(httpRequest);

                // grab the auth header from the inbound http request
                var authHeader = httpRequest.Headers["Authorization"];

                try
                {
                    // process the inbound activity with the bot
                    var invokeResponse = await ProcessActivityAsync(authHeader, activity, bot.OnTurnAsync, cancellationToken).ConfigureAwait(false);

                    // write the response, potentially serializing the InvokeResponse
                    HttpHelper.WriteResponse(httpResponse, invokeResponse);
                }
                catch (UnauthorizedAccessException)
                {
                    // handle unauthorized here as this layer creates the http response
                    httpResponse.StatusCode = (int)HttpStatusCode.Unauthorized;
                }
            }
        }

        /// <summary>
        /// Replaces the implementation in the base adapter in order to add
        /// support for streaming connections.
        /// </summary>
        /// <param name="turnContext">The current turn context.</param>
        /// <param name="activities">The collection of activities to send to the channel.</param>
        /// <param name="cancellationToken">Required cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public override async Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (activities == null)
            {
                throw new ArgumentNullException(nameof(activities));
            }

            var responses = new ResourceResponse[activities.Length];

            /*
             * NOTE: we're using for here (vs. foreach) because we want to simultaneously index into the
             * activities array to get the activity to process as well as use that index to assign
             * the response to the responses array and this is the most cost effective way to do that.
             */
            for (var index = 0; index < activities.Length; index++)
            {
                var activity = activities[index] ?? throw new ArgumentNullException("Found null activity in SendActivitiesAsync.");
                var response = default(ResourceResponse);
                _logger.LogInformation($"Sending activity.  ReplyToId: {activity.ReplyToId}");

                if (activity.Type == ActivityTypesEx.Delay)
                {
                    // The Activity Schema doesn't have a delay type build in, so it's simulated
                    // here in the Bot. This matches the behavior in the Node connector.
                    var delayMs = (int)activity.Value;
                    await Task.Delay(delayMs, cancellationToken).ConfigureAwait(false);

                    // No need to create a response. One will be created below.
                }
                else if (activity.Type == ActivityTypesEx.InvokeResponse)
                {
                    turnContext.TurnState.Add(InvokeResponseKey, activity);

                    // No need to create a response. One will be created below.
                }
                else if (activity.Type == ActivityTypes.Trace && activity.ChannelId != "emulator")
                {
                    // if it is a Trace activity we only send to the channel if it's the emulator.
                }
                else if (activity.ServiceUrl.StartsWith("u"))
                {
                    // The ServiceUrl for streaming channels begin with the string "urn" and contain
                    // information unique to streaming connections. If the ServiceUrl for this
                    // activity begins with a "u" we hand it off to be processed via a new or
                    // existing streaming connection.
                    response = await SendStreamingActivityAsync(activity);
                }
                else if (!string.IsNullOrWhiteSpace(activity.ReplyToId))
                {
                    var connectorClient = turnContext.TurnState.Get<IConnectorClient>();
                    response = await connectorClient.Conversations.ReplyToActivityAsync(activity, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    var connectorClient = turnContext.TurnState.Get<IConnectorClient>();
                    response = await connectorClient.Conversations.SendToConversationAsync(activity, cancellationToken).ConfigureAwait(false);
                }

                // If No response is set, then default to a "simple" response. This can't really be done
                // above, as there are cases where the ReplyTo/SendTo methods will also return null
                // (See below) so the check has to happen here.

                // Note: In addition to the Invoke / Delay / Activity cases, this code also applies
                // with Skype and Teams with regards to typing events.  When sending a typing event in
                // these _channels they do not return a RequestResponse which causes the bot to blow up.
                // https://github.com/Microsoft/botbuilder-dotnet/issues/460
                // bug report : https://github.com/Microsoft/botbuilder-dotnet/issues/465
                if (response == null)
                {
                    response = new ResourceResponse(activity.Id ?? string.Empty);
                }

                responses[index] = response;
            }

            return responses;
        }

        /// <summary>
        /// Primary adapter method for processing activities sent from streaming channel.
        /// Creates a turn context and runs the middleware pipeline for an incoming activity.
        /// Throws <see cref="ArgumentNullException"/> on null arguments.
        /// </summary>
        /// <param name="activity">The <see cref="Activity"/> to process.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute. If the activity type
        /// was 'Invoke' and the corresponding key (channelId + activityId) was found
        /// then an InvokeResponse is returned, otherwise null is returned.</returns>
        /// <remarks>Call this method to reactively send a message to a conversation.
        /// If the task completes successfully, then if the activity's <see cref="Activity.Type"/>
        /// is <see cref="ActivityTypes.Invoke"/> and the corresponding key
        /// (<see cref="Activity.ChannelId"/> + <see cref="Activity.Id"/>) is found
        /// then an <see cref="InvokeResponse"/> is returned, otherwise null is returned.
        /// <para>This method registers the following services for the turn.<list type="bullet"/></para>
        /// </remarks>
        public async Task<InvokeResponse> ProcessActivityForStreamingChannelAsync(Activity activity, CancellationToken cancellationToken = default)
        {
            BotAssert.ActivityNotNull(activity);

            _logger.LogInformation($"Received an incoming activity.  ActivityId: {activity.Id}");

            // If a conversation has moved from one connection to another for the same Channel or Skill and
            // hasn't been forgotten by the previous StreamingRequestHandler. The last requestHandler
            // the conversation has been associated with should always be the active connection.
            var requestHandler = _requestHandlers.Where(x => x.ServiceUrl == activity.ServiceUrl).Where(y => y.HasConversation(activity.Conversation.Id)).LastOrDefault();
            using (var context = new TurnContext(this, activity))
            {
                context.TurnState.Add<IIdentity>(BotIdentityKey, _claimsIdentity);
                var connectorClient = CreateStreamingConnectorClient(activity, requestHandler);
                context.TurnState.Add(connectorClient);

                await RunPipelineAsync(context, _bot.OnTurnAsync, cancellationToken).ConfigureAwait(false);

                if (activity.Type == ActivityTypes.Invoke)
                {
                    var activityInvokeResponse = context.TurnState.Get<Activity>(InvokeResponseKey);
                    if (activityInvokeResponse == null)
                    {
                        return new InvokeResponse { Status = (int)HttpStatusCode.NotImplemented };
                    }
                    else
                    {
                        return (InvokeResponse)activityInvokeResponse.Value;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Creates a new StreamingRequestHandler to listen to the specififed Named Pipe
        /// and pass requests to this adapter.
        /// </summary>
        /// <param name="pipeName">The name of the Named Pipe to connect to.</param>
        /// <param name="bot">The bot to use when processing activities received over the Named Pipe.</param>
        /// <returns>A task that completes only once the StreamingRequestHandler has stopped listening
        /// for incoming requests on the Named Pipe.</returns>
        public async Task AddNamedPipeConnection(string pipeName, IBot bot)
        {
            if (_requestHandlers == null)
            {
                _requestHandlers = new List<StreamingRequestHandler>();
            }

            _bot = bot ?? throw new ArgumentNullException(nameof(bot));
            var requestHandler = new StreamingRequestHandler(_logger, this, pipeName);
            _requestHandlers.Add(requestHandler);

            await requestHandler.StartListening();
        }

        /// <summary>
        /// Sends activities over streaming connections.
        /// If an existing connection is known the adapter will look for it and make use of it.
        /// If no existing connection is found a new connection will be opened.
        /// </summary>
        /// <param name="activity">The <see cref="Activity"/> to send.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that resolves to a <see cref="ResourceResponse"/>.</returns>
        private async Task<ResourceResponse> SendStreamingActivityAsync(Activity activity, CancellationToken cancellationToken = default)
        {
            // Check to see if any of this adapter's StreamingRequestHandlers is associated with this conversation.
            var possibleHandlers = _requestHandlers.Where(x => x.ServiceUrl == activity.ServiceUrl).Where(y => y.HasConversation(activity.Conversation.Id));

            if (possibleHandlers.Count() > 0)
            {
                if (possibleHandlers.Count() > 1)
                {
                    // The conversation has moved to a new connection and the former StreamingRequestHandler needs to be told to forget about it.
                    var correctHandler = possibleHandlers.OrderBy(x => x.ConversationAddedTime(activity.Conversation.Id)).Last();
                    foreach (var handler in possibleHandlers)
                    {
                        if (handler != correctHandler)
                        {
                            handler.ForgetConversation(activity.Conversation.Id);
                        }
                    }

                    return await correctHandler.SendActivityAsync(activity, cancellationToken);
                }

                return await possibleHandlers.FirstOrDefault().SendActivityAsync(activity, cancellationToken);
            }
            else
            {
                // This is a proactive message that will need a new streaming connection opened.
                // TODO: This connection needs authentication headers added to it.
                var connection = new ClientWebSocket();
                await connection.ConnectAsync(new Uri(activity.ServiceUrl), cancellationToken);
                var handler = new StreamingRequestHandler(_logger, this, connection);

                if (_requestHandlers == null)
                {
                    _requestHandlers = new List<StreamingRequestHandler>();
                }

                _requestHandlers.Add(handler);

                return await handler.SendActivityAsync(activity, cancellationToken);
            }
        }

        /// <summary>
        /// Process the initial request to establish a long lived connection via a streaming server.
        /// </summary>
        /// <param name="httpRequest">The connection request.</param>
        /// <param name="httpResponse">The response sent on error or connection termination.</param>
        /// <returns>Returns on task completion.</returns>
        private async Task ConnectWebSocket(HttpRequest httpRequest, HttpResponse httpResponse)
        {
            if (httpRequest == null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }

            if (httpResponse == null)
            {
                throw new ArgumentNullException(nameof(httpResponse));
            }

            if (!httpRequest.HttpContext.WebSockets.IsWebSocketRequest)
            {
                httpRequest.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await httpRequest.HttpContext.Response.WriteAsync("Upgrade to WebSocket is required.").ConfigureAwait(false);

                return;
            }

            if (!await AuthCheck(httpRequest))
            {
                return;
            }

            try
            {
                var socket = await httpRequest.HttpContext.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
                var requestHandler = new StreamingRequestHandler(_logger, this, socket);

                if (_requestHandlers == null)
                {
                    _requestHandlers = new List<StreamingRequestHandler>();
                }

                _requestHandlers.Add(requestHandler);

                await requestHandler.StartListening().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                httpRequest.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await httpRequest.HttpContext.Response.WriteAsync("Unable to create transport server.").ConfigureAwait(false);

                throw ex;
            }
        }

        private IConnectorClient CreateStreamingConnectorClient(Activity activity, StreamingRequestHandler requestHandler)
        {
            // TODO: When this is merged into the existing adapter it should be moved inside of
            // the existing CreateConnectorClient and use the serviceURL to determine which
            // version of the connector to construct.
            var emptyCredentials = (_channelProvider != null && _channelProvider.IsGovernment()) ?
                    MicrosoftGovernmentAppCredentials.Empty :
                    MicrosoftAppCredentials.Empty;
            var streamingClient = new StreamingHttpClient(requestHandler, _logger);
            var connectorClient = new ConnectorClient(new Uri(activity.ServiceUrl), emptyCredentials, customHttpClient: streamingClient);
            return connectorClient;
        }

        private async Task<bool> AuthCheck(HttpRequest httpRequest)
        {
            try
            {
                if (!await _credentialProvider.IsAuthenticationDisabledAsync().ConfigureAwait(false))
                {
                    var authHeader = httpRequest.Headers.Where(x => x.Key.ToLower() == AuthHeaderName).FirstOrDefault().Value.FirstOrDefault();
                    var channelId = httpRequest.Headers.Where(x => x.Key.ToLower() == ChannelIdHeaderName).FirstOrDefault().Value.FirstOrDefault();

                    if (string.IsNullOrWhiteSpace(authHeader))
                    {
                        await MissingAuthHeaderHelperAsync(AuthHeaderName, httpRequest).ConfigureAwait(false);

                        return false;
                    }

                    if (string.IsNullOrWhiteSpace(channelId))
                    {
                        await MissingAuthHeaderHelperAsync(ChannelIdHeaderName, httpRequest).ConfigureAwait(false);

                        return false;
                    }

                    var claimsIdentity = await JwtTokenValidation.ValidateAuthHeader(authHeader, _credentialProvider, _channelProvider, channelId).ConfigureAwait(false);
                    if (!claimsIdentity.IsAuthenticated)
                    {
                        httpRequest.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

                        return false;
                    }

                    _claimsIdentity = claimsIdentity;
                }

                return true;
            }
            catch (Exception ex)
            {
                httpRequest.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await httpRequest.HttpContext.Response.WriteAsync("Error while attempting to authorize connection.").ConfigureAwait(false);

                throw ex;
            }
        }

        private async Task MissingAuthHeaderHelperAsync(string headerName, HttpRequest httpRequest)
        {
            httpRequest.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await httpRequest.HttpContext.Response.WriteAsync($"Unable to authentiate. Missing header: {headerName}").ConfigureAwait(false);
        }
    }
}
