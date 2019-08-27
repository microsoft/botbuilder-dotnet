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
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.StreamingExtensions;
using Microsoft.Bot.StreamingExtensions.Payloads;
using Microsoft.Bot.StreamingExtensions.Transport;
using Microsoft.Bot.StreamingExtensions.Transport.NamedPipes;
using Microsoft.Bot.StreamingExtensions.Transport.WebSockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.StreamingExtensions
{
    /// <summary>
    /// Used to process incoming requests sent over an <see cref="IStreamingTransport"/> and adhering to the Bot Framework Protocol v3 with Streaming Extensions.
    /// </summary>
    public class DirectLineAdapter : BotFrameworkAdapter, IRequestHandler, IBotFrameworkHttpAdapter
    {
        /*  The default named pipe all instances of DL ASE listen on is named bfv4.pipes
        Unfortunately this name is no longer very discriptive, but for the time being
        we're unable to change it without coordinated updates to DL ASE, which we
        currently are unable to perform.
        */
        private const string DefaultPipeName = "bfv4.pipes";
        private const string AuthHeaderName = "authorization";
        private const string ChannelIdHeaderName = "channelid";
        private const string InvokeResponseKey = "DirectLineAdapter.InvokeResponse";
        private IStreamingTransportServer _transportServer;
        private string _userAgent;
        private IBot _bot;
        private readonly IChannelProvider _channelProvider;
        private readonly ICredentialProvider _credentialProvider;
        private readonly IList<IMiddleware> _middlewareSet;
        private readonly IServiceProvider _services;
        private readonly ILogger _logger;
        private HttpClient _httpClient;

        public DirectLineAdapter(ICredentialProvider credentialProvider, IChannelProvider channelProvider = null, ILogger logger = null)
            : base(credentialProvider, channelProvider)
        {
            _credentialProvider = credentialProvider;
            _channelProvider = channelProvider;
            _logger = logger ?? NullLogger.Instance;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectLineAdapter"/> class.
        /// The StreamingRequestHandler serves as a translation layer between the transport layer and bot adapter.
        /// It receives ReceiveRequests from the transport and provides them to the bot adapter in a form
        /// it is able to build activities out of, which are then handed to the bot itself to processed.
        /// Throws <see cref="ArgumentNullException"/> if arguments are null.
        /// </summary>
        /// <param name="onTurnError">Optional function to perform on turn errors.</param>
        /// <param name="bot">The <see cref="IBot"/> to be used for all requests to this handler.</param>
        /// <param name="middlewareSet">An optional set of middleware to register with the bot.</param>
        public DirectLineAdapter(Func<ITurnContext, Exception, Task> onTurnError, IBot bot, IList<IMiddleware> middlewareSet = null)
            : base(new SimpleCredentialProvider())
        {
            this.OnTurnError = onTurnError;
            _bot = bot ?? throw new ArgumentNullException(nameof(bot));
            _middlewareSet = middlewareSet ?? new List<IMiddleware>();
            _userAgent = GetUserAgent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectLineAdapter"/> class.
        /// An overload for use with dependency injection via ServiceProvider, as shown
        /// in DotNet Core Bot Builder samples.
        /// Throws <see cref="ArgumentNullException"/> if arguments are null.
        /// </summary>
        /// <param name="onTurnError">Optional function to perform on turn errors.</param>
        /// <param name="serviceProvider">The service collection containing the registered IBot type.</param>
        /// <param name="middlewareSet">An optional set of middleware to register with the bot.</param>
        public DirectLineAdapter(Func<ITurnContext, Exception, Task> onTurnError, IServiceProvider serviceProvider, IList<IMiddleware> middlewareSet = null)
             : base(new SimpleCredentialProvider())
        {
                this.OnTurnError = onTurnError;
                _services = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
                _middlewareSet = middlewareSet ?? new List<IMiddleware>();
                _userAgent = GetUserAgent();
            }

        /// <summary>
        /// Process the initial request to establish a long lived connection via a streaming server.
        /// </summary>
        /// <param name="httpRequest">The connection request.</param>
        /// <param name="httpResponse">The response sent on error or connection termination.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>Returns on task completion.</returns>
        public async Task ConnectWebSocket(HttpRequest httpRequest, HttpResponse httpResponse, CancellationToken cancellationToken = default(CancellationToken))
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

            try
            {
                if (!await _credentialProvider.IsAuthenticationDisabledAsync().ConfigureAwait(false))
                {
                    var authHeader = httpRequest.Headers.Where(x => x.Key.ToLower() == AuthHeaderName).FirstOrDefault().Value.FirstOrDefault();
                    var channelId = httpRequest.Headers.Where(x => x.Key.ToLower() == ChannelIdHeaderName).FirstOrDefault().Value.FirstOrDefault();

                    if (string.IsNullOrWhiteSpace(authHeader))
                    {
                        await MissingAuthHeaderHelperAsync(AuthHeaderName, httpRequest).ConfigureAwait(false);

                        return;
                    }

                    if (string.IsNullOrWhiteSpace(channelId))
                    {
                        await MissingAuthHeaderHelperAsync(ChannelIdHeaderName, httpRequest).ConfigureAwait(false);

                        return;
                    }

                    var claimsIdentity = await JwtTokenValidation.ValidateAuthHeader(authHeader, _credentialProvider, _channelProvider, channelId).ConfigureAwait(false);
                    if (!claimsIdentity.IsAuthenticated)
                    {
                        httpRequest.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                httpRequest.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await httpRequest.HttpContext.Response.WriteAsync("Error while attempting to authorize connection.").ConfigureAwait(false);

                throw ex;
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

        /// <summary>
        /// Connects the handler to a Named Pipe server and begins listening for incoming requests.
        /// </summary>
        /// <param name="pipeName">The name of the named pipe to use when creating the server.</param>
        /// <returns>A task that runs until the server is disconnected.</returns>
        public Task ConnectNamedPipe(string pipeName = DefaultPipeName)
        {
            _transportServer = new NamedPipeServer(pipeName, this);
            _httpClient = new StreamingHttpClient(_transportServer, _logger);

            return _transportServer.StartAsync();
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
        public async Task<StreamingResponse> ProcessRequestAsync(ReceiveRequest request, ILogger<IRequestHandler> logger, object context = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            logger = logger ?? NullLogger<IRequestHandler>.Instance;
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
                return await ProcessStreamingRequestAsync(request, response, logger, cancellationToken).ConfigureAwait(false);
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
        public async Task<InvokeResponse> ProcessActivityAsync(string body, List<IContentStream> streams, BotCallbackHandler callback, CancellationToken cancellationToken = default(CancellationToken))
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

            this._bot = bot;

            if (httpRequest.Method == HttpMethods.Get)
            {
                await ConnectWebSocket(httpRequest, httpResponse, cancellationToken).ConfigureAwait(false);
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
        /// Primary adapter method for processing activities sent from channel.
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
        public async Task<InvokeResponse> ProcessActivityAsync(Activity activity, BotCallbackHandler callback, CancellationToken cancellationToken = default(CancellationToken))
        {
            BotAssert.ActivityNotNull(activity);

            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            _logger.LogInformation($"Received an incoming activity.  ActivityId: {activity.Id}");

            using (var context = new TurnContext(this, activity))
            {
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
        /// <param name="logger">Optional logger.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The response ready to send to the client.</returns>
        private async Task<StreamingResponse> ProcessStreamingRequestAsync(ReceiveRequest request, StreamingResponse response, ILogger<IRequestHandler> logger, CancellationToken cancellationToken)
        {
            var body = string.Empty;

            try
            {
                body = request.ReadBodyAsString();
            }
            catch (Exception ex)
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                logger.LogError("Request body missing or malformed: " + ex.Message);

                return response;
            }

            try
            {
                IBot bot = null;

                // First check if an IBot type definition is available from the service provider.
                if (_services != null)
                {
                    /* Creating a new scope for each request allows us to support
                     * bots that inject scoped services as dependencies.
                     */
                    bot = _services.CreateScope().ServiceProvider.GetService<IBot>();
                }

                // If no bot has been set, check if a singleton bot is associated with this request handler.
                if (bot == null)
                {
                    bot = _bot;
                }

                // If a bot still hasn't been set, the request will not be handled correctly, so throw and terminate.
                if (bot == null)
                {
                    throw new Exception("Unable to find bot when processing request.");
                }

                var invokeResponse = await ProcessActivityAsync(body, request.Streams, new BotCallbackHandler(bot.OnTurnAsync), cancellationToken).ConfigureAwait(false);

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
                logger.LogError(ex.Message);
            }

            return response;
        }
    }
}
