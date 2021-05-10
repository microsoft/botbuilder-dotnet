// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Rest.TransientFaultHandling;

namespace Microsoft.Bot.Builder.Streaming
{
    /// <summary>
    /// An HTTP adapter base class.
    /// </summary>
    public class BotFrameworkHttpAdapterBase : BotFrameworkAdapter, IStreamingActivityProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BotFrameworkHttpAdapterBase"/> class.
        /// </summary>
        /// <param name="credentialProvider">The credential provider.</param>
        /// <param name="authConfig">The authentication configuration.</param>
        /// <param name="channelProvider">The channel provider.</param>
        /// <param name="connectorClientRetryPolicy">Retry policy for retyring HTTP operations.</param>
        /// <param name="customHttpClient">The HTTP client.</param>
        /// <param name="middleware">The middleware to initially add to the adapter.</param>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        public BotFrameworkHttpAdapterBase(
            ICredentialProvider credentialProvider,
            AuthenticationConfiguration authConfig,
            IChannelProvider channelProvider = null,
            RetryPolicy connectorClientRetryPolicy = null,
            HttpClient customHttpClient = null,
            IMiddleware middleware = null,
            ILogger logger = null)
            : base(credentialProvider, authConfig, channelProvider, connectorClientRetryPolicy, customHttpClient, middleware, logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BotFrameworkHttpAdapterBase"/> class.
        /// </summary>
        /// <param name="credentialProvider">The credential provider.</param>
        /// <param name="channelProvider">The channel provider.</param>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        public BotFrameworkHttpAdapterBase(ICredentialProvider credentialProvider = null, IChannelProvider channelProvider = null, ILogger<BotFrameworkHttpAdapterBase> logger = null)
            : this(credentialProvider ?? new SimpleCredentialProvider(), new AuthenticationConfiguration(), channelProvider, null, null, null, logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BotFrameworkHttpAdapterBase"/> class.
        /// </summary>
        /// <param name="credentialProvider">The credential provider.</param>
        /// <param name="channelProvider">The channel provider.</param>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        public BotFrameworkHttpAdapterBase(ICredentialProvider credentialProvider, IChannelProvider channelProvider, HttpClient httpClient, ILogger<BotFrameworkHttpAdapterBase> logger)
            : this(credentialProvider ?? new SimpleCredentialProvider(), new AuthenticationConfiguration(), channelProvider, null, httpClient, null, logger)
        {
        }

        /// <summary>
        /// Gets or sets the bot connected to this adapter.
        /// </summary>
        /// <value>
        /// The bot connected to this adapter.
        /// </value>
        protected IBot ConnectedBot { get; set; }

        /// <summary>
        /// Gets or sets the claims identity for this adapter.
        /// </summary>
        /// <value>
        /// The claims identity for this adapter.
        /// </value>
        protected ClaimsIdentity ClaimsIdentity { get; set; }

        /// <summary>
        /// Gets or sets the request handlers for this adapter.
        /// </summary>
        /// <value>
        /// The request handlers for this adapter.
        /// </value>
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        protected IList<StreamingRequestHandler> RequestHandlers { get; set; } = new List<StreamingRequestHandler>();
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Primary adapter method for processing activities sent from streaming channel.
        /// Creates a turn context and runs the middleware pipeline for an incoming activity.
        /// Throws <see cref="ArgumentNullException"/> on null arguments.
        /// </summary>
        /// <param name="activity">The <see cref="Activity"/> to process.</param>
        /// <param name="callbackHandler">The <see cref="BotCallbackHandler"/> that will handle the activity.</param>
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
        public async Task<InvokeResponse> ProcessStreamingActivityAsync(Activity activity, BotCallbackHandler callbackHandler, CancellationToken cancellationToken = default)
        {
            BotAssert.ActivityNotNull(activity);

            Logger.LogInformation($"Received an incoming streaming activity. ActivityId: {activity.Id}");
            
            // If a StreamingRequestHandler.Audience is a null value, then no callerId should have been generated
            // and GetAudienceFromCallerId returns null.
            // Thus we fallback to relying on the "original key", essentially $"{ServiceUrl}{Conversation.Id}",
            // as opposed to $"{ServiceUrl}{Audience}{Conversation.Id}" and the StreamingRequestHandler implicitly does not support skills.
            var audience = GetAudienceFromCallerId(activity);

            // If a conversation has moved from one connection to another for the same Channel or Skill and
            // hasn't been forgotten by the previous StreamingRequestHandler. The last requestHandler
            // the conversation has been associated with should always be the active connection.
            var requestHandler = RequestHandlers.Where(
                h => h.ServiceUrl == activity.ServiceUrl
                    && h.Audience == audience
                    && h.HasConversation(activity.Conversation.Id))
                .LastOrDefault();
            using (var context = new TurnContext(this, activity))
            {
                // TurnContextStateCollection applies a null check on value when using TurnContextStateCollection.Add().
                // TurnContextStateCollection.Set() doesn't perform the same null check, and is used in its place.
                // See https://github.com/microsoft/botbuilder-dotnet/issues/5110 for more information.
                context.TurnState.Set<string>(OAuthScopeKey, audience);

                // Pipes are unauthenticated. Pending to check that we are in pipes right now. Do not merge to master without that.
                if (ClaimsIdentity != null)
                {
                    context.TurnState.Add<IIdentity>(BotIdentityKey, ClaimsIdentity);
                }

                using (var connectorClient = CreateStreamingConnectorClient(activity, requestHandler))
                {
                    // Add connector client to be used throughout the turn
                    context.TurnState.Add(connectorClient);

                    await RunPipelineAsync(context, callbackHandler, cancellationToken).ConfigureAwait(false);

                    // Cleanup connector client 
                    context.TurnState.Set<IConnectorClient>(null);
                }

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
        /// Sends an activity.
        /// </summary>
        /// <param name="activity">>The <see cref="Activity"/> to send.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>If the task completes successfully, the result contains a the resource response object.</remarks>
        public async Task<ResourceResponse> SendStreamingActivityAsync(Activity activity, CancellationToken cancellationToken = default)
        {
            // Check to see if any of this adapter's StreamingRequestHandlers is associated with this conversation.
            var possibleHandlers = RequestHandlers.Where(x => x.ServiceUrl == activity.ServiceUrl).Where(y => y.HasConversation(activity.Conversation.Id));

            if (possibleHandlers.Any())
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

                    return await correctHandler.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);
                }

                return await possibleHandlers.First().SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);
            }

            if (ConnectedBot != null)
            {
                // This is a proactive message that will need a new streaming connection opened.
                // The ServiceUrl of a streaming connection follows the pattern "urn:[ChannelName]:[Protocol]:[Host]".
#pragma warning disable CA2000 // Dispose objects before losing scope (we can't fix this without closing the socket connection, this should be addressed after we make StreamingRequestHandler disposable and we dispose the connector )
                var connection = new ClientWebSocket();
#pragma warning restore CA2000 // Dispose objects before losing scope
                var uri = activity.ServiceUrl.Split(':');
                var protocol = uri[uri.Length - 2];
                var host = uri[uri.Length - 1];
                await connection.ConnectAsync(new Uri(protocol + host + "/api/messages"), cancellationToken).ConfigureAwait(false);

                var handler = new StreamingRequestHandler(ConnectedBot, this, connection, Logger);

                if (RequestHandlers == null)
                {
                    RequestHandlers = new List<StreamingRequestHandler>();
                }

                RequestHandlers.Add(handler);

                return await handler.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);
            }

            return null;
        }

        /// <summary>
        /// Creates a new StreamingRequestHandler to listen to the specified Named Pipe
        /// and pass requests to this adapter.
        /// </summary>
        /// <param name="pipeName">The name of the Named Pipe to connect to.</param>
        /// <param name="bot">The bot to use when processing activities received over the Named Pipe.</param>
        /// <param name="audience">The specified recipient of all outgoing activities.</param>
        /// <returns>A task that completes only once the StreamingRequestHandler has stopped listening
        /// for incoming requests on the Named Pipe.</returns>
        public async Task ConnectNamedPipeAsync(string pipeName, IBot bot, string audience = null)
        {
            if (string.IsNullOrEmpty(pipeName))
            {
                throw new ArgumentNullException(nameof(pipeName));
            }

            ConnectedBot = bot ?? throw new ArgumentNullException(nameof(bot));
            ClaimsIdentity = ClaimsIdentity ?? new ClaimsIdentity();

            if (RequestHandlers == null)
            {
                RequestHandlers = new List<StreamingRequestHandler>();
            }

            var requestHandler = new StreamingRequestHandler(bot, this, pipeName, audience, Logger);
            RequestHandlers.Add(requestHandler);

            await requestHandler.ListenAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Evaluates if processing an outgoing activity is possible.
        /// </summary>
        /// <remarks>If returns true, <see cref="BotFrameworkHttpAdapterBase.ProcessOutgoingActivityAsync"/> will be responsible for sending 
        /// the outgoing activity.</remarks>
        /// <param name="activity">The outgoing activity.</param>
        /// <returns>Whether should call ProcessOutgoingActivityAsync to send the outgoing activity.</returns>
        protected override bool CanProcessOutgoingActivity(Activity activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            return activity.IsFromStreamingConnection();
        }

        /// <summary>
        /// Sends an outgoing activity.
        /// </summary>
        /// <param name="turnContext">The context object for the turn.</param>
        /// <param name="activity">The activity to be processed.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of processing the activity.</returns>
        protected override async Task<ResourceResponse> ProcessOutgoingActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            // Check if we have token responses from OAuth cards.
            TokenResolver.CheckForOAuthCards(this, Logger, turnContext, activity, cancellationToken);

            // The ServiceUrl for streaming channels begins with the string "urn" and contains
            // information unique to streaming connections. Now that we know that this is a streaming
            // activity, process it in the streaming pipeline.
            // Process streaming activity.
            return await SendStreamingActivityAsync(activity, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a streaming specific connector client.
        /// </summary>
        private IConnectorClient CreateStreamingConnectorClient(Activity activity, StreamingRequestHandler requestHandler)
        {
            var emptyCredentials = (ChannelProvider != null && ChannelProvider.IsGovernment()) ?
                    MicrosoftGovernmentAppCredentials.Empty :
                    MicrosoftAppCredentials.Empty;
#pragma warning disable CA2000 // Dispose objects before losing scope (We need to make ConnectorClient disposable to fix this, ignoring it for now)
            var streamingClient = new StreamingHttpClient(requestHandler, Logger);
#pragma warning restore CA2000 // Dispose objects before losing scope
            var connectorClient = new ConnectorClient(new Uri(activity.ServiceUrl), emptyCredentials, customHttpClient: streamingClient, disposeHttpClient: false);
            return connectorClient;
        }

        /// <summary>
        /// Attempts to get an audience from the <see cref="Activity.CallerId"/>.
        /// </summary>
        /// <param name="activity">The incoming activity to be processed by a <see cref="StreamingRequestHandler"/>.</param>
        private string GetAudienceFromCallerId(Activity activity)
        {
            if (string.IsNullOrEmpty(activity.CallerId))
            {
                return null;
            }

            switch (activity.CallerId)
            {
                case CallerIdConstants.PublicAzureChannel:
                    return AuthenticationConstants.ToChannelFromBotOAuthScope;
                case CallerIdConstants.USGovChannel:
                    return GovernmentAuthenticationConstants.ToChannelFromBotOAuthScope;
                default:
                    if (activity.CallerId.StartsWith(CallerIdConstants.BotToBotPrefix, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return activity.CallerId.Substring(CallerIdConstants.BotToBotPrefix.Length);
                    }

                    return null;
            }
        }
    }
}
