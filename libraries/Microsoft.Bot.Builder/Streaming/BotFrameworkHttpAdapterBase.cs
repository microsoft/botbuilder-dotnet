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
    public class BotFrameworkHttpAdapterBase : BotFrameworkAdapter, IStreamingActivityProcessor
    {
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
        
        public BotFrameworkHttpAdapterBase(ICredentialProvider credentialProvider = null, IChannelProvider channelProvider = null, ILogger<BotFrameworkHttpAdapterBase> logger = null)
            : this(credentialProvider ?? new SimpleCredentialProvider(), new AuthenticationConfiguration(), channelProvider, null, null, null, logger)
        {
        }

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
        protected IList<StreamingRequestHandler> RequestHandlers { get; set; } = new List<StreamingRequestHandler>();

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

            // If a conversation has moved from one connection to another for the same Channel or Skill and
            // hasn't been forgotten by the previous StreamingRequestHandler. The last requestHandler
            // the conversation has been associated with should always be the active connection.
            var requestHandler = RequestHandlers.Where(x => x.ServiceUrl == activity.ServiceUrl).Where(y => y.HasConversation(activity.Conversation.Id)).LastOrDefault();
            using (var context = new TurnContext(this, activity))
            {
                // Pipes are unauthenticated. Pending to check that we are in pipes right now. Do not merge to master without that.
                if (ClaimsIdentity != null)
                {
                    context.TurnState.Add<IIdentity>(BotIdentityKey, ClaimsIdentity);
                }

                var connectorClient = CreateStreamingConnectorClient(activity, requestHandler);
                context.TurnState.Add(connectorClient);

                await RunPipelineAsync(context, callbackHandler, cancellationToken).ConfigureAwait(false);

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

        public async Task<ResourceResponse> SendStreamingActivityAsync(Activity activity, CancellationToken cancellationToken = default)
        {
            // Check to see if any of this adapter's StreamingRequestHandlers is associated with this conversation.
            var possibleHandlers = RequestHandlers.Where(x => x.ServiceUrl == activity.ServiceUrl).Where(y => y.HasConversation(activity.Conversation.Id));

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

                    return await correctHandler.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);
                }

                return await possibleHandlers.First().SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                if (ConnectedBot != null)
                {
                    // This is a proactive message that will need a new streaming connection opened.
                    // The ServiceUrl of a streaming connection follows the pattern "urn:[ChannelName]:[Protocol]:[Host]".
                    var connection = new ClientWebSocket();
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
        }

        protected override bool CanProcessOutgoingActivity(Activity activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            return activity.IsFromStreamingConnection();
        }

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
            return await SendStreamingActivityAsync(activity).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a streaming specific connector client.
        /// </summary>
        private IConnectorClient CreateStreamingConnectorClient(Activity activity, StreamingRequestHandler requestHandler)
        {
            var emptyCredentials = (ChannelProvider != null && ChannelProvider.IsGovernment()) ?
                    MicrosoftGovernmentAppCredentials.Empty :
                    MicrosoftAppCredentials.Empty;
            var streamingClient = new StreamingHttpClient(requestHandler, Logger);
            var connectorClient = new ConnectorClient(new Uri(activity.ServiceUrl), emptyCredentials, customHttpClient: streamingClient);
            return connectorClient;
        }
    }
}
