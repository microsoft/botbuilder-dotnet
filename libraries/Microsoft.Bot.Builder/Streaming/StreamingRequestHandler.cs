// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Streaming;
using Microsoft.Bot.Streaming.Transport;
using Microsoft.Bot.Streaming.Transport.NamedPipes;
using Microsoft.Bot.Streaming.Transport.WebSockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Streaming
{
    /// <summary>
    /// A request handler that processes incoming requests sent over an IStreamingTransport 
    /// and adheres to the Bot Framework Protocol v3 with Streaming Extensions.
    /// </summary>
    public class StreamingRequestHandler : RequestHandler
    {
        private readonly IBot _bot;
        private readonly ILogger _logger;
        private readonly IStreamingActivityProcessor _activityProcessor;
        private readonly string _userAgent;
        private readonly IDictionary<string, DateTime> _conversations;
        private readonly IStreamingTransportServer _server;

        private bool _serverIsConnected;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamingRequestHandler"/> class and
        /// establishes a connection over a WebSocket to a streaming channel.
        /// </summary>
        /// <param name="bot">The bot for which we handle requests.</param>
        /// <param name="activityProcessor">The processor for incoming requests.</param>
        /// <param name="socket">The base socket to use when connecting to the channel.</param>
        /// <param name="logger">Logger implementation for tracing and debugging information.</param>
        public StreamingRequestHandler(IBot bot, IStreamingActivityProcessor activityProcessor, WebSocket socket, ILogger logger = null)
            : this(bot, activityProcessor, socket, null, logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamingRequestHandler"/> class and
        /// establishes a connection over a WebSocket to a streaming channel.
        /// </summary>
        /// <remarks>
        /// The audience represents the recipient at the other end of the StreamingRequestHandler's
        /// streaming connection. Some acceptable audience values are as follows:
        /// <list>
        /// <item>- For Public Azure channels, use <see cref="Microsoft.Bot.Connector.Authentication.AuthenticationConstants.ToChannelFromBotOAuthScope"/>.</item>
        /// <item>- For Azure Government channels, use <see cref="Microsoft.Bot.Connector.Authentication.GovernmentAuthenticationConstants.ToChannelFromBotOAuthScope"/>.</item>
        /// </list>
        /// </remarks>
        /// <param name="bot">The bot for which we handle requests.</param>
        /// <param name="activityProcessor">The processor for incoming requests.</param>
        /// <param name="socket">The base socket to use when connecting to the channel.</param>
        /// <param name="logger">Logger implementation for tracing and debugging information.</param>
        /// <param name="audience">The specified recipient of all outgoing activities.</param>
        public StreamingRequestHandler(IBot bot, IStreamingActivityProcessor activityProcessor, WebSocket socket, string audience, ILogger logger = null)
        {
            _bot = bot ?? throw new ArgumentNullException(nameof(bot));
            _activityProcessor = activityProcessor ?? throw new ArgumentNullException(nameof(activityProcessor));

            if (socket == null)
            {
                throw new ArgumentNullException(nameof(socket));
            }

            Audience = audience;
            _logger = logger ?? NullLogger.Instance;
            _conversations = new ConcurrentDictionary<string, DateTime>();
            _userAgent = GetUserAgent();
            _server = new WebSocketServer(socket, this);
            _serverIsConnected = true;
            _server.Disconnected += Server_Disconnected;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamingRequestHandler"/> class and
        /// establishes a connection over a Named Pipe to a streaming channel.
        /// </summary>
        /// <param name="bot">The bot for which we handle requests.</param>
        /// <param name="activityProcessor">The processor for incoming requests.</param>
        /// <param name="pipeName">The name of the Named Pipe to use when connecting to the channel.</param>
        /// <param name="logger">Logger implementation for tracing and debugging information.</param>
        public StreamingRequestHandler(IBot bot, IStreamingActivityProcessor activityProcessor, string pipeName, ILogger logger = null)
            : this(bot, activityProcessor, pipeName, null, logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamingRequestHandler"/> class and
        /// establishes a connection over a Named Pipe to a streaming channel.
        /// </summary>
        /// <remarks>
        /// The audience represents the recipient at the other end of the StreamingRequestHandler's
        /// streaming connection. Some acceptable audience values are as follows:
        /// <list>
        /// <item>- For Public Azure channels, use <see cref="Microsoft.Bot.Connector.Authentication.AuthenticationConstants.ToChannelFromBotOAuthScope"/>.</item>
        /// <item>- For Azure Government channels, use <see cref="Microsoft.Bot.Connector.Authentication.GovernmentAuthenticationConstants.ToChannelFromBotOAuthScope"/>.</item>
        /// </list>
        /// </remarks>
        /// <param name="bot">The bot for which we handle requests.</param>
        /// <param name="activityProcessor">The processor for incoming requests.</param>
        /// <param name="pipeName">The name of the Named Pipe to use when connecting to the channel.</param>
        /// <param name="logger">Logger implementation for tracing and debugging information.</param>
        /// <param name="audience">The specified recipient of all outgoing activities.</param>
        public StreamingRequestHandler(IBot bot, IStreamingActivityProcessor activityProcessor, string pipeName, string audience, ILogger logger = null)
        {
            _bot = bot ?? throw new ArgumentNullException(nameof(bot));
            _activityProcessor = activityProcessor ?? throw new ArgumentNullException(nameof(activityProcessor));
            _logger = logger ?? NullLogger.Instance;

            if (string.IsNullOrWhiteSpace(pipeName))
            {
                throw new ArgumentNullException(nameof(pipeName));
            }

            Audience = audience;
            _conversations = new ConcurrentDictionary<string, DateTime>();
            _userAgent = GetUserAgent();
            _server = new NamedPipeServer(pipeName, this);
            _serverIsConnected = true;
            _server.Disconnected += Server_Disconnected;
        }

        /// <summary>
        /// Gets the URL of the channel endpoint this StreamingRequestHandler receives requests from.
        /// </summary>
        /// <value>
        /// The URL of the channel endpoint this StreamingRequestHandler receives requests from.
        /// </value>
#pragma warning disable CA1056 // Uri properties should not be strings (we can't change this without breaking binary compat)
        public string ServiceUrl { get; private set; }
#pragma warning restore CA1056 // Uri properties should not be strings

        /// <summary>
        /// Gets the intended recipient of <see cref="Activity">Activities</see> sent from this StreamingRequestHandler.
        /// </summary>
        /// <value>
        /// The intended recipient of Activities sent from this StreamingRequestHandler.
        /// </value>
        public string Audience { get; private set; }

        /// <summary>
        /// Begins listening for incoming requests over this StreamingRequestHandler's server.
        /// </summary>
        /// <returns>A task that completes once the server is no longer listening.</returns>
        public async Task ListenAsync()
        {
            await _server.StartAsync().ConfigureAwait(false);
            _logger.LogInformation("Streaming request handler started listening");
        }

        /// <summary>
        /// Checks to see if the set of conversations this StreamingRequestHandler has
        /// received requests for contains the passed in conversation ID.
        /// </summary>
        /// <param name="conversationId">The ID of the conversation to check for.</param>
        /// <returns>True if the conversation ID was found in this StreamingRequestHandler's conversation set.</returns>
        public bool HasConversation(string conversationId)
        {
            return _conversations.ContainsKey(conversationId);
        }

        /// <summary>
        /// Gets the <see cref="DateTime"/> when the conversation was added to this request handler.
        /// </summary>
        /// <param name="conversationId">The id of the conversation.</param>
        /// <returns><see cref="DateTime.MinValue"/> if conversation is not found, otherwise the <see cref="DateTime"/>
        /// the conversation was added to this <see cref="StreamingRequestHandler"/>.</returns>
        public DateTime ConversationAddedTime(string conversationId)
        {
            if (!_conversations.TryGetValue(conversationId, out var addedTime))
            {
                addedTime = DateTime.MinValue;
            }

            return addedTime;
        }

        /// <summary>
        /// Removes the given conversation from this instance of the StreamingRequestHandler's collection
        /// of tracked conversations.
        /// </summary>
        /// <param name="conversationId">The ID of the conversation to remove.</param>
        public void ForgetConversation(string conversationId)
        {
            _conversations.Remove(conversationId);
        }

        /// <summary>
        /// Handles incoming requests.
        /// </summary>
        /// <param name="request">A <see cref="ReceiveRequest"/> for this handler to process.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="context">Optional context to process the request within.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="Task"/> that will produce a <see cref="StreamingResponse"/> on successful completion.</returns>
        public override async Task<StreamingResponse> ProcessRequestAsync(ReceiveRequest request, ILogger<RequestHandler> logger = null, object context = null, CancellationToken cancellationToken = default)
        {
            var response = new StreamingResponse();

            // We accept all POSTs regardless of path, but anything else requires special treatment.
            if (!string.Equals(request?.Verb, StreamingRequest.POST, StringComparison.OrdinalIgnoreCase))
            {
                return HandleCustomPaths(request, response);
            }

            // Convert the StreamingRequest into an activity the adapter can understand.
            string body;
            try
            {
                body = await request.ReadBodyAsStringAsync().ConfigureAwait(false);
            }
#pragma warning disable CA1031 // Do not catch general exception types (we log the exception and continue execution)
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                _logger.LogError("Request body missing or malformed: " + ex.Message);

                return response;
            }

            try
            {
                var activity = JsonConvert.DeserializeObject<Activity>(body, SerializationSettings.DefaultDeserializationSettings);

                // All activities received by this StreamingRequestHandler will originate from the same channel, but we won't
                // know what that channel is until we've received the first request.
                if (string.IsNullOrWhiteSpace(ServiceUrl))
                {
                    ServiceUrl = activity.ServiceUrl;
                }

                // If this is the first time the handler has seen this conversation it needs to be added to the dictionary so the
                // adapter is able to route requests to the correct handler.
                if (!HasConversation(activity.Conversation.Id))
                {
                    _conversations.Add(activity.Conversation.Id, DateTime.Now);
                }

                /*
                 * Any content sent as part of a StreamingRequest, including the request body
                 * and inline attachments, appear as streams added to the same collection. The first
                 * stream of any request will be the body, which is parsed and passed into this method
                 * as the first argument, 'body'. Any additional streams are inline attachments that need
                 * to be iterated over and added to the Activity as attachments to be sent to the Bot.
                 */
                if (request.Streams.Count > 1)
                {
                    var streamAttachments = new List<Attachment>();
                    for (var i = 1; i < request.Streams.Count; i++)
                    {
                        streamAttachments.Add(new Attachment() { ContentType = request.Streams[i].ContentType, Content = request.Streams[i].Stream });
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

                // Populate Activity.CallerId given the Audience value.
                string callerId = null;
                switch (Audience)
                {
                    case AuthenticationConstants.ToChannelFromBotOAuthScope:
                        callerId = CallerIdConstants.PublicAzureChannel;
                        break;
                    case GovernmentAuthenticationConstants.ToChannelFromBotOAuthScope:
                        callerId = CallerIdConstants.USGovChannel;
                        break;
                    default:
                        if (!string.IsNullOrEmpty(Audience))
                        {
                            if (Guid.TryParse(Audience, out var result))
                            {
                                // Large assumption drawn here; any GUID is an AAD AppId. This is prohibitive towards bots not using the Bot Framework auth model
                                // but still using GUIDs/UUIDs as identifiers.
                                // It's also indicative of the tight coupling between the Bot Framework protocol, authentication and transport mechanism in the SDK.
                                // In R12, this work will be re-implemented to better utilize the CallerId and Audience set on BotFrameworkAuthentication instances
                                // and decouple the three concepts mentioned above.
                                callerId = $"{CallerIdConstants.BotToBotPrefix}{Audience}";
                            }
                            else
                            {
                                // Fallback to using the raw Audience as the CallerId. The auth model being used by the Adapter using this StreamingRequestHandler
                                // is not known to the SDK, therefore it is assumed the developer knows what they're doing. The SDK should not prevent
                                // the developer from extending it to use custom auth models in Streaming contexts.
                                callerId = Audience;
                            }
                        }

                        // A null Audience is an implicit statement indicating the bot does not support skills.
                        break;
                }

                activity.CallerId = callerId;

                // Now that the request has been converted into an activity we can send it to the adapter.
                var adapterResponse = await _activityProcessor.ProcessStreamingActivityAsync(activity, _bot.OnTurnAsync, cancellationToken).ConfigureAwait(false);

                // Now we convert the invokeResponse returned by the adapter into a StreamingResponse we can send back to the channel.
                if (adapterResponse == null)
                {
                    response.StatusCode = (int)HttpStatusCode.OK;
                }
                else
                {
                    response.StatusCode = adapterResponse.Status;
                    if (adapterResponse.Body != null)
                    {
                        response.SetBody(adapterResponse.Body);
                    }
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types (we logging the error and we send it back in the body of the response)
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.SetBody(ex.ToString());
                _logger.LogError(ex.ToString());
            }

            return response;
        }

        /// <summary>
        /// Converts an <see cref="Activity"/> into a <see cref="StreamingRequest"/> and sends it to the
        /// channel this StreamingRequestHandler is connected to.
        /// </summary>
        /// <param name="activity">The activity to send.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that resolves to a <see cref="ResourceResponse"/>.</returns>
        public async Task<ResourceResponse> SendActivityAsync(Activity activity, CancellationToken cancellationToken = default)
        {
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
                if (!_serverIsConnected)
                {
                    throw new Exception("Error while attempting to send: Streaming transport is disconnected.");
                }

                var serverResponse = await _server.SendAsync(request, cancellationToken).ConfigureAwait(false);

                if (serverResponse.StatusCode == (int)HttpStatusCode.OK)
                {
                    return serverResponse.ReadBodyAsJson<ResourceResponse>();
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types (this should probably be addressed later, but for now we just log the error and continue the execution)
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                _logger.LogError(ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Sends a <see cref="StreamingRequest"/> to the connected streaming channel.
        /// </summary>
        /// <param name="request">The request to send.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that resolves to a <see cref="ReceiveResponse"/>.</returns>
        public async Task<ReceiveResponse> SendStreamingRequestAsync(StreamingRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_serverIsConnected)
                {
                    throw new Exception("Error while attempting to send: Streaming transport is disconnected.");
                }

                var serverResponse = await _server.SendAsync(request, cancellationToken).ConfigureAwait(false);

                if (serverResponse.StatusCode == (int)HttpStatusCode.OK)
                {
                    return serverResponse.ReadBodyAsJson<ReceiveResponse>();
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types (this should probably be addressed later, but for now we just log the error and continue the execution)
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                _logger.LogError(ex.Message);
            }

            return null;
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
        private static string GetUserAgent()
        {
            using (var connectorClient = new ConnectorClient(new Uri("http://localhost")))
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "Microsoft-BotFramework/3.1 Streaming-Extensions/1.0 BotBuilder/{0} ({1}; {2}; {3})",
                    ConnectorClient.GetClientVersion(connectorClient),
                    ConnectorClient.GetASPNetVersion(),
                    ConnectorClient.GetOsVersion(),
                    ConnectorClient.GetArchitecture());
            }
        }

        private static IEnumerable<HttpContent> UpdateAttachmentStreams(Activity activity)
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

        private void Server_Disconnected(object sender, DisconnectedEventArgs e)
        {
            _serverIsConnected = false;
        }

        /// <summary>
        /// Checks the validity of the request and attempts to map it the correct custom endpoint,
        /// then generates and returns a response if appropriate.
        /// </summary>
        /// <param name="request">A ReceiveRequest from the connected channel.</param>
        /// <param name="response">The <see cref="StreamingResponse"/> instance.</param>
        /// <returns>A response if the given request matches against a defined path.</returns>
        private StreamingResponse HandleCustomPaths(ReceiveRequest request, StreamingResponse response)
        {
            if (request == null || string.IsNullOrEmpty(request.Verb) || string.IsNullOrEmpty(request.Path))
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                _logger.LogError("Request missing verb and/or path.");

                return response;
            }

            if (string.Equals(request.Verb, StreamingRequest.GET, StringComparison.OrdinalIgnoreCase) &&
                         string.Equals(request.Path, "/api/version", StringComparison.OrdinalIgnoreCase))
            {
                response.StatusCode = (int)HttpStatusCode.OK;
                response.SetBody(new VersionInfo() { UserAgent = _userAgent });

                return response;
            }

            return null;
        }
    }
}
