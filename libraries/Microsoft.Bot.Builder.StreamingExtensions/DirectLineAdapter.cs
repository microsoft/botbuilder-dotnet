// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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
using Microsoft.Bot.StreamingExtensions;
using Microsoft.Bot.StreamingExtensions.Payloads;
using Microsoft.Bot.StreamingExtensions.Transport;
using Microsoft.Bot.StreamingExtensions.Transport.NamedPipes;
using Microsoft.Bot.StreamingExtensions.Transport.WebSockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.StreamingExtensions
{
    /// <summary>
    /// A Bot Builder Adapter implementation used to handle Bot Framework HTTP and streaming requests. Supports the Bot Framework Protocol v3 with Streaming Extensions.
    /// </summary>
    public class DirectLineAdapter : BotFrameworkAdapter, IRequestHandler, IBotFrameworkHttpAdapter
    {
        private const string DefaultPipeName = "bfv4.pipes";
        private const string AuthHeaderName = "authorization";
        private const string ChannelIdHeaderName = "channelid";
        private const string InvokeResponseKey = "DirectLineAdapter.InvokeResponse";
        private const string BotIdentityKey = "BotIdentity";
        private readonly string _userAgent;
        private readonly IChannelProvider _channelProvider;
        private readonly ICredentialProvider _credentialProvider;
        private readonly ILogger _logger;
        private IBot _bot;
        private ClaimsIdentity _claimsIdentity;
        private HttpClient _httpClient;
        private IStreamingTransportServer _transportServer;

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
            _userAgent = GetUserAgent();
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
        /// Initializes a new instance of the <see cref="DirectLineAdapter"/> class for processing streaming requests over Named Pipe connections.
        /// Throws <see cref="ArgumentNullException"/> if arguments are null.
        /// </summary>
        /// <param name="onTurnError">Optional function to perform on turn errors.</param>
        /// <param name="bot">The <see cref="IBot"/> to be used for all requests to this handler.</param>
        /// <param name="pipeName">The name of the named pipe to use when creating the server.</param>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        public DirectLineAdapter(Func<ITurnContext, Exception, Task> onTurnError, IBot bot, string pipeName = null, ILogger<BotFrameworkHttpAdapter> logger = null)
             : base(new SimpleCredentialProvider())
        {
            if (string.IsNullOrWhiteSpace(pipeName))
            {
                pipeName = DefaultPipeName;
            }

            OnTurnError = onTurnError;
            _bot = bot ?? throw new ArgumentNullException(nameof(bot));
            _transportServer = new NamedPipeServer(pipeName, this);
            _httpClient = new StreamingHttpClient(_transportServer, _logger);
            _claimsIdentity = new ClaimsIdentity();
            if (logger != null)
            {
                _logger = logger;
            }
            else
            {
                _logger = NullLogger.Instance;
            }

            _userAgent = GetUserAgent();
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
        /// Returns an adapter configured for use with the specified Named Pipe.
        /// Unlike other connections the Named Pipe connection is created as part of the Bot's startup procedure,
        /// this requires exposing a method of binding a Named Pipe to a specific adapter instance prior to any
        /// messages being received. In line with the typical patterns used in Bot startup configurations this
        /// method allows for a dependency injection friendly path of newing up an adapter for use with a
        /// streaming connection over a Named Pipe.
        /// </summary>
        /// <param name="onTurnError">Optional function to perform on turn errors.</param>
        /// <param name="bot">The <see cref="IBot"/> to be used for all requests to this handler.</param>
        /// <param name="pipeName">The name of the named pipe to use when creating the server.</param>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        /// <returns>A task that runs until the server is disconnected.</returns>
        public DirectLineAdapter CreateAdapterListeningOnNamedPipe(Func<ITurnContext, Exception, Task> onTurnError, IBot bot, string pipeName = DefaultPipeName, ILogger<BotFrameworkHttpAdapter> logger = null)
        {
            var adapter = new DirectLineAdapter(onTurnError, bot, pipeName, logger);

            // We don't want to await for the connection to complete, as that means the session is over.
            // In the DI model there is nothing waiting for the session to complete, so there is no
            // need to watch for the completion of this task. Simply start the session and hand
            // up the adapter.
            _ = adapter.ConnectNamedPipe();

            return adapter;
        }

        /// <summary>
        /// Checks the validity of the request and attempts to map it the correct virtual endpoint,
        /// then generates and returns a response if appropriate.
        /// </summary>
        /// <param name="request">A ReceiveRequest from the connected channel.</param>
        /// <param name="logger">Optional logger used to log request information and error details.</param>
        /// <param name="context">Optional context to operate within. Unused in bot implementation.</param>
        /// /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A response created by the BotAdapter to be sent to the client that originated the request.</returns>
        public async Task<StreamingResponse> ProcessRequestAsync(ReceiveRequest request, ILogger logger, object context = null, CancellationToken cancellationToken = default)
        {
            // If a specific logger is passed in, use it for this request. Otherwise use the adapter's logger.
            if (logger == null)
            {
                logger = _logger;
            }

            var response = new StreamingResponse();

            if (request == null || string.IsNullOrEmpty(request.Verb) || string.IsNullOrEmpty(request.Path))
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                logger.LogError("Request missing verb and/or path.");

                return response;
            }

            if (string.Equals(request.Verb, StreamingRequest.GET, StringComparison.InvariantCultureIgnoreCase) &&
                         string.Equals(request.Path, "/api/version", StringComparison.InvariantCultureIgnoreCase))
            {
                response.StatusCode = (int)HttpStatusCode.OK;
                response.SetBody(new VersionInfo() { UserAgent = _userAgent });

                return response;
            }

            if (string.Equals(request.Verb, StreamingRequest.POST, StringComparison.InvariantCultureIgnoreCase) &&
                         string.Equals(request.Path, "/api/messages", StringComparison.InvariantCultureIgnoreCase))
            {
                return await ProcessStreamingRequestAsync(request, response, cancellationToken).ConfigureAwait(false);
            }

            response.StatusCode = (int)HttpStatusCode.NotFound;
            logger.LogError($"Unknown verb and path: {request.Verb} {request.Path}");

            return response;
        }

        /// <summary>
        /// Overload for processing activities when given the activity a json string representation of a request body.
        /// Creates a turn context and runs the middleware pipeline for an incoming activity.
        /// Throws <see cref="ArgumentNullException"/> on null arguments.
        /// </summary>
        /// <param name="body">The json string to deserialize into an <see cref="Activity"/>.</param>
        /// <param name="streams">A set of streams associated with but not attached to the <see cref="Activity"/>.</param>
        /// <param name="callback">The code to run at the end of the adapter's middleware pipeline.</param>
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
        public async Task<InvokeResponse> ProcessActivityAsync(string body, List<IContentStream> streams, BotCallbackHandler callback, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                throw new ArgumentNullException(nameof(body));
            }

            if (streams == null)
            {
                throw new ArgumentNullException(nameof(streams));
            }

            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            var activity = JsonConvert.DeserializeObject<Activity>(body, SerializationSettings.DefaultDeserializationSettings);

            /*
             * Any content sent as part of a StreamingRequest, including the request body
             * and inline attachments, appear as streams added to the same collection. The first
             * stream of any request will be the body, which is parsed and passed into this method
             * as the first argument, 'body'. Any additional streams are inline attachents that need
             * to be iterated over and added to the Activity as attachments to be sent to the Bot.
             */
            if (streams.Count > 1)
            {
                var streamAttachments = new List<Attachment>();
                for (var i = 1; i < streams.Count; i++)
                {
                    streamAttachments.Add(new Attachment() { ContentType = streams[i].ContentType, Content = streams[i].Stream });
                }

                if (activity.Attachments != null)
                {
                    activity.Attachments = activity.Attachments.Concat(streamAttachments).ToArray();
                }
                else
                {
                    activity.Attachments = streamAttachments.ToArray();
                }
            }

            return await ProcessActivityAsync(activity, callback, cancellationToken).ConfigureAwait(false);
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
        /// Overrides HTTP implementation in the base adapter, but defers to it if the conversation is not associated with a streaming connection.
        /// </summary>
        /// <param name="turnContext">The current turn context.</param>
        /// <param name="activities">The collection of activities to send to the channel.</param>
        /// <param name="cancellationToken">Required cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public override async Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)
        {
            if (!(_httpClient is StreamingHttpClient))
            {
                return await base.SendActivitiesAsync(turnContext, activities, cancellationToken);
            }

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

                string requestPath;
                if (!string.IsNullOrWhiteSpace(activity.ReplyToId) && activity.ReplyToId.Length >= 1)
                {
                    requestPath = $"/v3/conversations/{activity.Conversation?.Id}/activities/{activity.ReplyToId}";
                }
                else
                {
                    requestPath = $"/v3/conversations/{activity.Conversation?.Id}/activities";
                }

                var streamAttachments = UpdateAttachmentStreams(activity);
                var request = StreamingRequest.CreatePost(requestPath);
                request.SetBody(activity);
                if (streamAttachments != null)
                {
                    foreach (var attachment in streamAttachments)
                    {
                        request.AddStream(attachment);
                    }
                }

                try
                {
                    var serverResponse = await _transportServer.SendAsync(request, cancellationToken).ConfigureAwait(false);

                    if (serverResponse.StatusCode == (int)HttpStatusCode.OK)
                    {
                        response = serverResponse.ReadBodyAsJson<ResourceResponse>();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
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
        /// <param name="callback">The code to run at the end of the adapter's middleware pipeline.</param>
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
        public async Task<InvokeResponse> ProcessActivityAsync(Activity activity, BotCallbackHandler callback, CancellationToken cancellationToken = default)
        {
            BotAssert.ActivityNotNull(activity);

            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            _logger.LogInformation($"Received an incoming activity.  ActivityId: {activity.Id}");

            using (var context = new TurnContext(this, activity))
            {
                context.TurnState.Add<IIdentity>(BotIdentityKey, _claimsIdentity);
                var connectorClient = CreateStreamingConnectorClient(activity);
                context.TurnState.Add(connectorClient);

                await RunPipelineAsync(context, callback, cancellationToken).ConfigureAwait(false);

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
        /// Build and return versioning information used for telemetry, including:
        /// The Schema version is 3.1, put into the Microsoft-BotFramework header,
        /// Protocol Extension Info,
        /// The Client SDK Version
        ///  https://github.com/Microsoft/botbuilder-dotnet/blob/d342cd66d159a023ac435aec0fdf791f93118f5f/doc/UserAgents.md,
        /// Additional Info.
        /// https://github.com/Microsoft/botbuilder-dotnet/blob/d342cd66d159a023ac435aec0fdf791f93118f5f/doc/UserAgents.md.
        /// </summary>
        /// <returns>A string containing versioning information.</returns>
        private static string GetUserAgent() =>
            string.Format(
                "Microsoft-BotFramework/3.1 Streaming-Extensions/1.0 BotBuilder/{0} ({1}; {2}; {3})",
                ConnectorClient.GetClientVersion(new ConnectorClient(new Uri("http://localhost"))),
                ConnectorClient.GetASPNetVersion(),
                ConnectorClient.GetOsVersion(),
                ConnectorClient.GetArchitecture());

        /// <summary>
        /// Starts the adapter listening to the named pipe created.
        /// </summary>
        /// <returns>A task.</returns>
        private async Task ConnectNamedPipe() => await _transportServer.StartAsync();

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
                _transportServer = new WebSocketServer(socket, this);
                _httpClient = new StreamingHttpClient(_transportServer, _logger);

                await _transportServer.StartAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                httpRequest.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await httpRequest.HttpContext.Response.WriteAsync("Unable to create transport server.").ConfigureAwait(false);

                throw ex;
            }
        }

        private IConnectorClient CreateStreamingConnectorClient(Activity activity)
        {
            var emptyCredentials = (_channelProvider != null && _channelProvider.IsGovernment()) ?
                    MicrosoftGovernmentAppCredentials.Empty :
                    MicrosoftAppCredentials.Empty;
            var connectorClient = new ConnectorClient(new Uri(activity.ServiceUrl), emptyCredentials, customHttpClient: _httpClient);
            return connectorClient;
        }

        private IEnumerable<HttpContent> UpdateAttachmentStreams(Activity activity)
    {
        if (activity == null || activity.Attachments == null)
        {
            return null;
        }

        var streamAttachments = activity.Attachments.Where(a => a.Content is Stream);
        if (streamAttachments.Any())
        {
            activity.Attachments = activity.Attachments.Where(a => !(a.Content is Stream)).ToList();
            return streamAttachments.Select(streamAttachment =>
            {
                var streamContent = new StreamContent(streamAttachment.Content as Stream);
                streamContent.Headers.TryAddWithoutValidation("Content-Type", streamAttachment.ContentType);
                return streamContent;
            });
        }

        return null;
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

        /// <summary>
        /// Performs the actual processing of a request, handing it off to the adapter and returning the response.
        /// </summary>
        /// <param name="request">A ReceiveRequest from the connected channel.</param>
        /// <param name="response">The response to update and return, ultimately sent to client.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The response ready to send to the client.</returns>
        private async Task<StreamingResponse> ProcessStreamingRequestAsync(ReceiveRequest request, StreamingResponse response, CancellationToken cancellationToken)
        {
            var body = string.Empty;

            try
            {
                body = request.ReadBodyAsString();
            }
            catch (Exception ex)
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                _logger.LogError("Request body missing or malformed: " + ex.Message);

                return response;
            }

            try
            {
                var bot = _bot ?? throw new Exception("Unable to find bot when processing request.");
                var invokeResponse = await ProcessActivityAsync(body, request.Streams, _bot.OnTurnAsync, cancellationToken).ConfigureAwait(false);

                if (invokeResponse == null)
                {
                    response.StatusCode = (int)HttpStatusCode.OK;
                }
                else
                {
                    response.StatusCode = invokeResponse.Status;
                    if (invokeResponse.Body != null)
                    {
                        response.SetBody(invokeResponse.Body);
                    }
                }

                invokeResponse = null;
            }
            catch (Exception ex)
            {
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                _logger.LogError(ex.Message);
            }

            return response;
        }
    }
}
