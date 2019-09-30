// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Rest;
using Microsoft.Rest.TransientFaultHandling;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// A bot adapter that can connect a bot to a service endpoint.
    /// </summary>
    /// <remarks>The bot adapter encapsulates authentication processes and sends
    /// activities to and receives activities from the Bot Connector Service. When your
    /// bot receives an activity, the adapter creates a context object, passes it to your
    /// bot's application logic, and sends responses back to the user's channel.
    /// <para>Use <see cref="Use(IMiddleware)"/> to add <see cref="IMiddleware"/> objects
    /// to your adapter’s middleware collection. The adapter processes and directs
    /// incoming activities in through the bot middleware pipeline to your bot’s logic
    /// and then back out again. As each activity flows in and out of the bot, each piece
    /// of middleware can inspect or act upon the activity, both before and after the bot
    /// logic runs.</para>
    /// </remarks>
    /// <seealso cref="ITurnContext"/>
    /// <seealso cref="IActivity"/>
    /// <seealso cref="IBot"/>
    /// <seealso cref="IMiddleware"/>
    public class BotFrameworkAdapter : BotAdapter, IAdapterIntegration, IUserTokenProvider
    {
        private const string InvokeResponseKey = "BotFrameworkAdapter.InvokeResponse";
        private const string BotIdentityKey = "BotIdentity";

        private static readonly HttpClient _defaultHttpClient = new HttpClient();
        private readonly ICredentialProvider _credentialProvider;
        private readonly AppCredentials _appCredentials;
        private readonly IChannelProvider _channelProvider;
        private readonly HttpClient _httpClient;
        private readonly RetryPolicy _connectorClientRetryPolicy;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, AppCredentials> _appCredentialMap = new ConcurrentDictionary<string, AppCredentials>();
        private readonly AuthenticationConfiguration _authConfiguration;

        // There is a significant boost in throughput if we reuse a connectorClient
        // _connectorClients is a cache using [serviceUrl + appId].
        private readonly ConcurrentDictionary<string, ConnectorClient> _connectorClients = new ConcurrentDictionary<string, ConnectorClient>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BotFrameworkAdapter"/> class,
        /// using a credential provider.
        /// </summary>
        /// <param name="credentialProvider">The credential provider.</param>
        /// <param name="channelProvider">The channel provider.</param>
        /// <param name="connectorClientRetryPolicy">Retry policy for retrying HTTP operations.</param>
        /// <param name="customHttpClient">The HTTP client.</param>
        /// <param name="middleware">The middleware to initially add to the adapter.</param>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="credentialProvider"/> is <c>null</c>.</exception>
        /// <remarks>Use a <see cref="MiddlewareSet"/> object to add multiple middleware
        /// components in the constructor. Use the <see cref="Use(IMiddleware)"/> method to
        /// add additional middleware to the adapter after construction.
        /// </remarks>
        public BotFrameworkAdapter(
            ICredentialProvider credentialProvider,
            IChannelProvider channelProvider = null,
            RetryPolicy connectorClientRetryPolicy = null,
            HttpClient customHttpClient = null,
            IMiddleware middleware = null,
            ILogger logger = null)
            : this(credentialProvider, new AuthenticationConfiguration(), channelProvider, connectorClientRetryPolicy, customHttpClient, middleware, logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BotFrameworkAdapter"/> class,
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
        /// components in the constructor. Use the <see cref="Use(IMiddleware)"/> method to
        /// add additional middleware to the adapter after construction.
        /// </remarks>
        public BotFrameworkAdapter(
            ICredentialProvider credentialProvider,
            AuthenticationConfiguration authConfig,
            IChannelProvider channelProvider = null,
            RetryPolicy connectorClientRetryPolicy = null,
            HttpClient customHttpClient = null,
            IMiddleware middleware = null,
            ILogger logger = null)
        {
            _credentialProvider = credentialProvider ?? throw new ArgumentNullException(nameof(credentialProvider));
            _channelProvider = channelProvider;
            _httpClient = customHttpClient ?? _defaultHttpClient;
            _connectorClientRetryPolicy = connectorClientRetryPolicy;
            _logger = logger ?? NullLogger.Instance;
            _authConfiguration = authConfig ?? throw new ArgumentNullException(nameof(authConfig));

            if (middleware != null)
            {
                Use(middleware);
            }

            // Relocate the tenantId field used by MS Teams to a new location (from channelData to conversation)
            // This will only occur on activities from teams that include tenant info in channelData but NOT in conversation,
            // thus should be future friendly.  However, once the transition is complete. we can remove this.
            Use(new TenantIdWorkaroundForTeamsMiddleware());

            // DefaultRequestHeaders are not thread safe so set them up here because this adapter should be a singleton.
            ConnectorClient.AddDefaultRequestHeaders(_httpClient);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BotFrameworkAdapter"/> class,
        /// using a credential provider.
        /// </summary>
        /// <param name="credentials">The credentials to be used for token acquisition.</param>
        /// <param name="authConfig">The authentication configuration.</param>
        /// <param name="channelProvider">The channel provider.</param>
        /// <param name="connectorClientRetryPolicy">Retry policy for retrying HTTP operations.</param>
        /// <param name="customHttpClient">The HTTP client.</param>
        /// <param name="middleware">The middleware to initially add to the adapter.</param>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="credentialProvider"/> is <c>null</c>.</exception>
        /// <remarks>Use a <see cref="MiddlewareSet"/> object to add multiple middleware
        /// components in the constructor. Use the <see cref="Use(IMiddleware)"/> method to
        /// add additional middleware to the adapter after construction.
        /// </remarks>
        public BotFrameworkAdapter(
            AppCredentials credentials,
            AuthenticationConfiguration authConfig,
            IChannelProvider channelProvider = null,
            RetryPolicy connectorClientRetryPolicy = null,
            HttpClient customHttpClient = null,
            IMiddleware middleware = null,
            ILogger logger = null)
        {
            _appCredentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
            _credentialProvider = new SimpleCredentialProvider(credentials.MicrosoftAppId, string.Empty);
            _channelProvider = channelProvider;
            _httpClient = customHttpClient ?? _defaultHttpClient;
            _connectorClientRetryPolicy = connectorClientRetryPolicy;
            _logger = logger ?? NullLogger.Instance;
            _authConfiguration = authConfig ?? throw new ArgumentNullException(nameof(authConfig));

            if (middleware != null)
            {
                Use(middleware);
            }

            // Relocate the tenantId field used by MS Teams to a new location (from channelData to conversation)
            // This will only occur on activities from teams that include tenant info in channelData but NOT in conversation,
            // thus should be future friendly.  However, once the transition is complete. we can remove this.
            Use(new TenantIdWorkaroundForTeamsMiddleware());

            // DefaultRequestHeaders are not thread safe so set them up here because this adapter should be a singleton.
            ConnectorClient.AddDefaultRequestHeaders(_httpClient);
        }

        /// <summary>
        /// Sends a proactive message from the bot to a conversation.
        /// </summary>
        /// <param name="botAppId">The application ID of the bot. This is the appId returned by Portal registration, and is
        /// generally found in the "MicrosoftAppId" parameter in appSettings.json.</param>
        /// <param name="reference">A reference to the conversation to continue.</param>
        /// <param name="callback">The method to call for the resulting bot turn.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="botAppId"/>, <paramref name="reference"/>, or
        /// <paramref name="callback"/> is <c>null</c>.</exception>
        /// <remarks>Call this method to proactively send a message to a conversation.
        /// Most _channels require a user to initialize a conversation with a bot
        /// before the bot can send activities to the user.
        /// <para>This method registers the following services for the turn.<list type="bullet">
        /// <item><description><see cref="IIdentity"/> (key = "BotIdentity"), a claims identity for the bot.
        /// </description></item>
        /// <item><description><see cref="IConnectorClient"/>, the channel connector client to use this turn.
        /// </description></item>
        /// </list></para>
        /// <para>
        /// This overload differs from the Node implementation by requiring the BotId to be
        /// passed in. The .Net code allows multiple bots to be hosted in a single adapter which
        /// isn't something supported by Node.
        /// </para>
        /// </remarks>
        /// <seealso cref="ProcessActivityAsync(string, Activity, BotCallbackHandler, CancellationToken)"/>
        /// <seealso cref="BotAdapter.RunPipelineAsync(ITurnContext, BotCallbackHandler, CancellationToken)"/>
        public override async Task ContinueConversationAsync(string botAppId, ConversationReference reference, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(botAppId))
            {
                throw new ArgumentNullException(nameof(botAppId));
            }

            if (reference == null)
            {
                throw new ArgumentNullException(nameof(reference));
            }

            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            _logger.LogInformation($"Sending proactive message.  botAppId: {botAppId}");

            using (var context = new TurnContext(this, reference.GetContinuationActivity()))
            {
                // Hand craft Claims Identity.
                var claimsIdentity = new ClaimsIdentity(new List<Claim>
                {
                    // Adding claims for both Emulator and Channel.
                    new Claim(AuthenticationConstants.AudienceClaim, botAppId),
                    new Claim(AuthenticationConstants.AppIdClaim, botAppId),
                });

                context.TurnState.Add<IIdentity>(BotIdentityKey, claimsIdentity);
                var connectorClient = await CreateConnectorClientAsync(reference.ServiceUrl, claimsIdentity, cancellationToken).ConfigureAwait(false);
                context.TurnState.Add(connectorClient);
                await RunPipelineAsync(context, callback, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Adds middleware to the adapter's pipeline.
        /// </summary>
        /// <param name="middleware">The middleware to add.</param>
        /// <returns>The updated adapter object.</returns>
        /// <remarks>Middleware is added to the adapter at initialization time.
        /// For each turn, the adapter calls middleware in the order in which you added it.
        /// </remarks>
        public new BotFrameworkAdapter Use(IMiddleware middleware)
        {
            MiddlewareSet.Use(middleware);
            return this;
        }

        /// <summary>
        /// Creates a turn context and runs the middleware pipeline for an incoming activity.
        /// </summary>
        /// <param name="authHeader">The HTTP authentication header of the request.</param>
        /// <param name="activity">The incoming activity.</param>
        /// <param name="callback">The code to run at the end of the adapter's middleware pipeline.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute. If the activity type
        /// was 'Invoke' and the corresponding key (channelId + activityId) was found
        /// then an InvokeResponse is returned, otherwise null is returned.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="activity"/> is <c>null</c>.</exception>
        /// <exception cref="UnauthorizedAccessException">authentication failed.</exception>
        /// <remarks>Call this method to reactively send a message to a conversation.
        /// If the task completes successfully, then if the activity's <see cref="Activity.Type"/>
        /// is <see cref="ActivityTypes.Invoke"/> and the corresponding key
        /// (<see cref="Activity.ChannelId"/> + <see cref="Activity.Id"/>) is found
        /// then an <see cref="InvokeResponse"/> is returned, otherwise null is returned.
        /// <para>This method registers the following services for the turn.<list type="bullet">
        /// <item><see cref="IIdentity"/> (key = "BotIdentity"), a claims identity for the bot.</item>
        /// <item><see cref="IConnectorClient"/>, the channel connector client to use this turn.</item>
        /// </list></para>
        /// </remarks>
        /// <seealso cref="ContinueConversationAsync(string, ConversationReference, BotCallbackHandler, CancellationToken)"/>
        /// <seealso cref="BotAdapter.RunPipelineAsync(ITurnContext, BotCallbackHandler, CancellationToken)"/>
        public async Task<InvokeResponse> ProcessActivityAsync(string authHeader, Activity activity, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            BotAssert.ActivityNotNull(activity);

            var claimsIdentity = await JwtTokenValidation.AuthenticateRequest(activity, authHeader, _credentialProvider, _channelProvider, _authConfiguration, _httpClient).ConfigureAwait(false);
            return await ProcessActivityAsync(claimsIdentity, activity, callback, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a turn context and runs the middleware pipeline for an incoming activity.
        /// </summary>
        /// <param name="identity">A <see cref="ClaimsIdentity"/> for the request.</param>
        /// <param name="activity">The incoming activity.</param>
        /// <param name="callback">The code to run at the end of the adapter's middleware pipeline.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task<InvokeResponse> ProcessActivityAsync(ClaimsIdentity identity, Activity activity, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            BotAssert.ActivityNotNull(activity);

            _logger.LogInformation($"Received an incoming activity.  ActivityId: {activity.Id}");

            using (var context = new TurnContext(this, activity))
            {
                context.TurnState.Add<IIdentity>(BotIdentityKey, identity);

                var connectorClient = await CreateConnectorClientAsync(activity.ServiceUrl, identity, cancellationToken).ConfigureAwait(false);
                context.TurnState.Add(connectorClient);

                await RunPipelineAsync(context, callback, cancellationToken).ConfigureAwait(false);

                // Handle Invoke scenarios, which deviate from the request/response model in that
                // the Bot will return a specific body and return code.
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

                // For all non-invoke scenarios, the HTTP layers above don't have to mess
                // with the Body and return codes.
                return null;
            }
        }

        /// <summary>
        /// Sends activities to the conversation.
        /// </summary>
        /// <param name="turnContext">The context object for the turn.</param>
        /// <param name="activities">The activities to send.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the activities are successfully sent, the task result contains
        /// an array of <see cref="ResourceResponse"/> objects containing the IDs that
        /// the receiving channel assigned to the activities.</remarks>
        /// <seealso cref="ITurnContext.OnSendActivities(SendActivitiesHandler)"/>
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

            if (activities.Length == 0)
            {
                throw new ArgumentException("Expecting one or more activities, but the array was empty.", nameof(activities));
            }

            var responses = new ResourceResponse[activities.Length];

            /*
             * NOTE: we're using for here (vs. foreach) because we want to simultaneously index into the
             * activities array to get the activity to process as well as use that index to assign
             * the response to the responses array and this is the most cost effective way to do that.
             */
            for (var index = 0; index < activities.Length; index++)
            {
                var activity = activities[index];
                var response = default(ResourceResponse);

                _logger.LogInformation($"Sending activity.  ReplyToId: {activity.ReplyToId}");

                if (activity.Type == ActivityTypesEx.Delay)
                {
                    // The Activity Schema doesn't have a delay type build in, so it's simulated
                    // here in the Bot. This matches the behavior in the Node connector.
                    int delayMs = (int)activity.Value;
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
        /// Replaces an existing activity in the conversation.
        /// </summary>
        /// <param name="turnContext">The context object for the turn.</param>
        /// <param name="activity">New replacement activity.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the activity is successfully sent, the task result contains
        /// a <see cref="ResourceResponse"/> object containing the ID that the receiving
        /// channel assigned to the activity.
        /// <para>Before calling this, set the ID of the replacement activity to the ID
        /// of the activity to replace.</para></remarks>
        /// <seealso cref="ITurnContext.OnUpdateActivity(UpdateActivityHandler)"/>
        public override async Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            var connectorClient = turnContext.TurnState.Get<IConnectorClient>();
            return await connectorClient.Conversations.UpdateActivityAsync(activity, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes an existing activity in the conversation.
        /// </summary>
        /// <param name="turnContext">The context object for the turn.</param>
        /// <param name="reference">Conversation reference for the activity to delete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>The <see cref="ConversationReference.ActivityId"/> of the conversation
        /// reference identifies the activity to delete.</remarks>
        /// <seealso cref="ITurnContext.OnDeleteActivity(DeleteActivityHandler)"/>
        public override async Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken)
        {
            var connectorClient = turnContext.TurnState.Get<IConnectorClient>();
            await connectorClient.Conversations.DeleteActivityAsync(reference.Conversation.Id, reference.ActivityId, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Removes a member from the current conversation.
        /// </summary>
        /// <param name="turnContext">The context object for the turn.</param>
        /// <param name="memberId">The ID of the member to remove from the conversation.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task DeleteConversationMemberAsync(ITurnContext turnContext, string memberId, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Conversation == null)
            {
                throw new ArgumentNullException("BotFrameworkAdapter.deleteConversationMember(): missing conversation");
            }

            if (string.IsNullOrWhiteSpace(turnContext.Activity.Conversation.Id))
            {
                throw new ArgumentNullException("BotFrameworkAdapter.deleteConversationMember(): missing conversation.id");
            }

            var connectorClient = turnContext.TurnState.Get<IConnectorClient>();

            string conversationId = turnContext.Activity.Conversation.Id;

            await connectorClient.Conversations.DeleteConversationMemberAsync(conversationId, memberId, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Lists the members of a given activity.
        /// </summary>
        /// <param name="turnContext">The context object for the turn.</param>
        /// <param name="activityId">(Optional) Activity ID to enumerate. If not specified the current activities ID will be used.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of Members of the activity.</returns>
        public async Task<IList<ChannelAccount>> GetActivityMembersAsync(ITurnContext turnContext, string activityId, CancellationToken cancellationToken)
        {
            // If no activity was passed in, use the current activity.
            if (activityId == null)
            {
                activityId = turnContext.Activity.Id;
            }

            if (turnContext.Activity.Conversation == null)
            {
                throw new ArgumentNullException("BotFrameworkAdapter.GetActivityMembers(): missing conversation");
            }

            if (string.IsNullOrWhiteSpace(turnContext.Activity.Conversation.Id))
            {
                throw new ArgumentNullException("BotFrameworkAdapter.GetActivityMembers(): missing conversation.id");
            }

            var connectorClient = turnContext.TurnState.Get<IConnectorClient>();
            var conversationId = turnContext.Activity.Conversation.Id;

            IList<ChannelAccount> accounts = await connectorClient.Conversations.GetActivityMembersAsync(conversationId, activityId, cancellationToken).ConfigureAwait(false);

            return accounts;
        }

        /// <summary>
        /// Lists the members of the current conversation.
        /// </summary>
        /// <param name="turnContext">The context object for the turn.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of Members of the current conversation.</returns>
        public async Task<IList<ChannelAccount>> GetConversationMembersAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Conversation == null)
            {
                throw new ArgumentNullException("BotFrameworkAdapter.GetActivityMembers(): missing conversation");
            }

            if (string.IsNullOrWhiteSpace(turnContext.Activity.Conversation.Id))
            {
                throw new ArgumentNullException("BotFrameworkAdapter.GetActivityMembers(): missing conversation.id");
            }

            var connectorClient = turnContext.TurnState.Get<IConnectorClient>();
            var conversationId = turnContext.Activity.Conversation.Id;

            IList<ChannelAccount> accounts = await connectorClient.Conversations.GetConversationMembersAsync(conversationId, cancellationToken).ConfigureAwait(false);
            return accounts;
        }

        /// <summary>
        /// Lists the Conversations in which this bot has participated for a given channel server. The
        /// channel server returns results in pages and each page will include a `continuationToken`
        /// that can be used to fetch the next page of results from the server.
        /// </summary>
        /// <param name="serviceUrl">The URL of the channel server to query.  This can be retrieved
        /// from `context.activity.serviceUrl`. </param>
        /// <param name="credentials">The credentials needed for the Bot to connect to the services.</param>
        /// <param name="continuationToken">The continuation token from the previous page of results.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task completes successfully, the result contains a page of the members of the current conversation.
        /// This overload may be called from outside the context of a conversation, as only the
        /// bot's service URL and credentials are required.
        /// </remarks>
        public async Task<ConversationsResult> GetConversationsAsync(string serviceUrl, MicrosoftAppCredentials credentials, string continuationToken, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(serviceUrl))
            {
                throw new ArgumentNullException(nameof(serviceUrl));
            }

            if (credentials == null)
            {
                throw new ArgumentNullException(nameof(credentials));
            }

            var connectorClient = CreateConnectorClient(serviceUrl, credentials);
            var results = await connectorClient.Conversations.GetConversationsAsync(continuationToken, cancellationToken).ConfigureAwait(false);
            return results;
        }

        /// <summary>
        /// Lists the Conversations in which this bot has participated for a given channel server. The
        /// channel server returns results in pages and each page will include a `continuationToken`
        /// that can be used to fetch the next page of results from the server.
        /// </summary>
        /// <param name="turnContext">The context object for the turn.</param>
        /// <param name="continuationToken">The continuation token from the previous page of results.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task completes successfully, the result contains a page of the members of the current conversation.
        /// This overload may be called during standard activity processing, at which point the Bot's
        /// service URL and credentials that are part of the current activity processing pipeline
        /// will be used.
        /// </remarks>
        public async Task<ConversationsResult> GetConversationsAsync(ITurnContext turnContext, string continuationToken, CancellationToken cancellationToken)
        {
            var connectorClient = turnContext.TurnState.Get<IConnectorClient>();
            var results = await connectorClient.Conversations.GetConversationsAsync(continuationToken, cancellationToken).ConfigureAwait(false);
            return results;
        }

        /// <summary>
        /// Attempts to retrieve the token for a user that's in a login flow.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="magicCode">(Optional) Optional user entered code to validate.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Token Response.</returns>
        public virtual async Task<TokenResponse> GetUserTokenAsync(ITurnContext turnContext, string connectionName, string magicCode, CancellationToken cancellationToken)
        {
            BotAssert.ContextNotNull(turnContext);
            if (turnContext.Activity.From == null || string.IsNullOrWhiteSpace(turnContext.Activity.From.Id))
            {
                throw new ArgumentNullException("BotFrameworkAdapter.GetUserTokenAsync(): missing from or from.id");
            }

            if (string.IsNullOrWhiteSpace(connectionName))
            {
                throw new ArgumentNullException(nameof(connectionName));
            }

            var client = await CreateOAuthApiClientAsync(turnContext).ConfigureAwait(false);
            return await client.UserToken.GetTokenAsync(turnContext.Activity.From.Id, connectionName, turnContext.Activity.ChannelId, magicCode, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the raw signin link to be sent to the user for signin for a connection name.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task completes successfully, the result contains the raw signin link.</remarks>
        public virtual async Task<string> GetOauthSignInLinkAsync(ITurnContext turnContext, string connectionName, CancellationToken cancellationToken)
        {
            BotAssert.ContextNotNull(turnContext);
            if (string.IsNullOrWhiteSpace(connectionName))
            {
                throw new ArgumentNullException(nameof(connectionName));
            }

            var activity = turnContext.Activity;

            var tokenExchangeState = new TokenExchangeState()
            {
                ConnectionName = connectionName,
                Conversation = new ConversationReference()
                {
                    ActivityId = activity.Id,
                    Bot = activity.Recipient,       // Activity is from the user to the bot
                    ChannelId = activity.ChannelId,
                    Conversation = activity.Conversation,
                    ServiceUrl = activity.ServiceUrl,
                    User = activity.From,
                },
                MsAppId = (_credentialProvider as MicrosoftAppCredentials)?.MicrosoftAppId,
            };

            var serializedState = JsonConvert.SerializeObject(tokenExchangeState);
            var encodedState = Encoding.UTF8.GetBytes(serializedState);
            var state = Convert.ToBase64String(encodedState);

            var client = await CreateOAuthApiClientAsync(turnContext).ConfigureAwait(false);
            return await client.BotSignIn.GetSignInUrlAsync(state, null, null, null, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the raw signin link to be sent to the user for signin for a connection name.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="userId">The user id that will be associated with the token.</param>
        /// <param name="finalRedirect">The final URL that the OAuth flow will redirect to.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task completes successfully, the result contains the raw signin link.</remarks>
        public virtual async Task<string> GetOauthSignInLinkAsync(ITurnContext turnContext, string connectionName, string userId, string finalRedirect = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            BotAssert.ContextNotNull(turnContext);

            if (string.IsNullOrWhiteSpace(connectionName))
            {
                throw new ArgumentNullException(nameof(connectionName));
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            var tokenExchangeState = new TokenExchangeState()
            {
                ConnectionName = connectionName,
                Conversation = new ConversationReference()
                {
                    ActivityId = null,
                    Bot = new ChannelAccount { Role = "bot" },
                    ChannelId = Channels.Directline,
                    Conversation = new ConversationAccount(),
                    ServiceUrl = null,
                    User = new ChannelAccount { Role = "user", Id = userId, },
                },
                MsAppId = (_credentialProvider as MicrosoftAppCredentials)?.MicrosoftAppId,
            };

            var serializedState = JsonConvert.SerializeObject(tokenExchangeState);
            var encodedState = Encoding.UTF8.GetBytes(serializedState);
            var state = Convert.ToBase64String(encodedState);

            var client = await CreateOAuthApiClientAsync(turnContext).ConfigureAwait(false);
            return await client.BotSignIn.GetSignInUrlAsync(state, null, null, finalRedirect, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Signs the user out with the token server.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="userId">User id of user to sign out.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public virtual async Task SignOutUserAsync(ITurnContext turnContext, string connectionName = null, string userId = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            BotAssert.ContextNotNull(turnContext);

            if (string.IsNullOrEmpty(userId))
            {
                userId = turnContext.Activity?.From?.Id;
            }

            var client = await CreateOAuthApiClientAsync(turnContext).ConfigureAwait(false);
            await client.UserToken.SignOutAsync(userId, connectionName, turnContext.Activity?.ChannelId, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves the token status for each configured connection for the given user.
        /// </summary>
        /// <param name="context">Context for the current turn of conversation with the user.</param>
        /// <param name="userId">The user Id for which token status is retrieved.</param>
        /// <param name="includeFilter">Optional comma separated list of connection's to include. Blank will return token status for all configured connections.</param>
        /// <param name="cancellationToken">The async operation cancellation token.</param>
        /// <returns>Array of TokenStatus.</returns>
        public virtual async Task<TokenStatus[]> GetTokenStatusAsync(ITurnContext context, string userId, string includeFilter = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            BotAssert.ContextNotNull(context);

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            var client = await CreateOAuthApiClientAsync(context).ConfigureAwait(false);
            var result = await client.UserToken.GetTokenStatusAsync(userId, context.Activity?.ChannelId, includeFilter, cancellationToken).ConfigureAwait(false);
            return result?.ToArray();
        }

        /// <summary>
        /// Retrieves Azure Active Directory tokens for particular resources on a configured connection.
        /// </summary>
        /// <param name="context">Context for the current turn of conversation with the user.</param>
        /// <param name="connectionName">The name of the Azure Active Directory connection configured with this bot.</param>
        /// <param name="resourceUrls">The list of resource URLs to retrieve tokens for.</param>
        /// <param name="userId">The user Id for which tokens are retrieved. If passing in null the userId is taken from the Activity in the ITurnContext.</param>
        /// <param name="cancellationToken">The async operation cancellation token.</param>
        /// <returns>Dictionary of resourceUrl to the corresponding TokenResponse.</returns>
        public virtual async Task<Dictionary<string, TokenResponse>> GetAadTokensAsync(ITurnContext context, string connectionName, string[] resourceUrls, string userId = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            BotAssert.ContextNotNull(context);

            if (string.IsNullOrWhiteSpace(connectionName))
            {
                throw new ArgumentNullException(nameof(connectionName));
            }

            if (resourceUrls == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                userId = context.Activity?.From?.Id;
            }

            var client = await CreateOAuthApiClientAsync(context).ConfigureAwait(false);
            return (Dictionary<string, TokenResponse>)await client.UserToken.GetAadTokensAsync(userId, connectionName, new AadResourceUrls() { ResourceUrls = resourceUrls?.ToList() }, context.Activity?.ChannelId, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a conversation on the specified channel.
        /// </summary>
        /// <param name="channelId">The ID for the channel.</param>
        /// <param name="serviceUrl">The channel's service URL endpoint.</param>
        /// <param name="credentials">The application credentials for the bot.</param>
        /// <param name="conversationParameters">The conversation information to use to
        /// create the conversation.</param>
        /// <param name="callback">The method to call for the resulting bot turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>To start a conversation, your bot must know its account information
        /// and the user's account information on that channel.
        /// Most _channels only support initiating a direct message (non-group) conversation.
        /// <para>The adapter attempts to create a new conversation on the channel, and
        /// then sends a <c>conversationUpdate</c> activity through its middleware pipeline
        /// to the <paramref name="callback"/> method.</para>
        /// <para>If the conversation is established with the
        /// specified users, the ID of the activity's <see cref="IActivity.Conversation"/>
        /// will contain the ID of the new conversation.</para>
        /// </remarks>
        public virtual async Task CreateConversationAsync(string channelId, string serviceUrl, MicrosoftAppCredentials credentials, ConversationParameters conversationParameters, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            var connectorClient = CreateConnectorClient(serviceUrl, credentials);

            var result = await connectorClient.Conversations.CreateConversationAsync(conversationParameters, cancellationToken).ConfigureAwait(false);

            // Create a conversation update activity to represent the result.
            var eventActivity = Activity.CreateEventActivity();
            eventActivity.Name = "CreateConversation";
            eventActivity.ChannelId = channelId;
            eventActivity.ServiceUrl = serviceUrl;
            eventActivity.Id = result.ActivityId ?? Guid.NewGuid().ToString("n");
            eventActivity.Conversation = new ConversationAccount(id: result.Id, tenantId: conversationParameters.TenantId);
            eventActivity.ChannelData = conversationParameters.ChannelData;
            eventActivity.Recipient = conversationParameters.Bot;

            using (TurnContext context = new TurnContext(this, (Activity)eventActivity))
            {
                ClaimsIdentity claimsIdentity = new ClaimsIdentity();
                claimsIdentity.AddClaim(new Claim(AuthenticationConstants.AudienceClaim, credentials.MicrosoftAppId));
                claimsIdentity.AddClaim(new Claim(AuthenticationConstants.AppIdClaim, credentials.MicrosoftAppId));
                claimsIdentity.AddClaim(new Claim(AuthenticationConstants.ServiceUrlClaim, serviceUrl));

                context.TurnState.Add<IIdentity>(BotIdentityKey, claimsIdentity);
                context.TurnState.Add(connectorClient);
                await RunPipelineAsync(context, callback, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Creates a conversation on the specified channel. Overload receives a ConversationReference including the tenant.
        /// </summary>
        /// <param name="channelId">The ID for the channel.</param>
        /// <param name="serviceUrl">The channel's service URL endpoint.</param>
        /// <param name="credentials">The application credentials for the bot.</param>
        /// <param name="conversationParameters">The conversation information to use to
        /// create the conversation.</param>
        /// <param name="callback">The method to call for the resulting bot turn.</param>
        /// <param name="reference">A conversation reference that contains the tenant.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>To start a conversation, your bot must know its account information
        /// and the user's account information on that channel.
        /// Most _channels only support initiating a direct message (non-group) conversation.
        /// <para>The adapter attempts to create a new conversation on the channel, and
        /// then sends a <c>conversationUpdate</c> activity through its middleware pipeline
        /// to the <paramref name="callback"/> method.</para>
        /// <para>If the conversation is established with the
        /// specified users, the ID of the activity's <see cref="IActivity.Conversation"/>
        /// will contain the ID of the new conversation.</para>
        /// </remarks>
        public virtual async Task CreateConversationAsync(string channelId, string serviceUrl, MicrosoftAppCredentials credentials, ConversationParameters conversationParameters, BotCallbackHandler callback, ConversationReference reference, CancellationToken cancellationToken)
        {
            if (reference.Conversation != null)
            {
                var tenantId = reference.Conversation.TenantId;

                if (tenantId != null)
                {
                    // Putting tenantId in channelData is a temporary solution while we wait for the Teams API to be updated
                    conversationParameters.ChannelData = new { tenant = new { tenantId = tenantId.ToString() } };

                    // Permanent solution is to put tenantId in parameters.tenantId
                    conversationParameters.TenantId = tenantId.ToString();
                }

                await CreateConversationAsync(channelId, serviceUrl, credentials, conversationParameters, callback, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Creates an OAuth client for the bot.
        /// </summary>
        /// <param name="turnContext">The context object for the current turn.</param>
        /// <returns>An OAuth client for the bot.</returns>
        protected virtual async Task<OAuthClient> CreateOAuthApiClientAsync(ITurnContext turnContext)
        {
            if (!OAuthClientConfig.EmulateOAuthCards &&
                string.Equals(turnContext.Activity.ChannelId, "emulator", StringComparison.InvariantCultureIgnoreCase) &&
                (await _credentialProvider.IsAuthenticationDisabledAsync().ConfigureAwait(false)))
            {
                OAuthClientConfig.EmulateOAuthCards = true;
            }

            var connectorClient = turnContext.TurnState.Get<IConnectorClient>();
            if (connectorClient == null)
            {
                throw new InvalidOperationException("An IConnectorClient is required in TurnState for this operation.");
            }

            if (OAuthClientConfig.EmulateOAuthCards)
            {
                // do not await task - we want this to run in the background
                var oauthClient = new OAuthClient(new Uri(turnContext.Activity.ServiceUrl), connectorClient.Credentials);
                var task = Task.Run(() => OAuthClientConfig.SendEmulateOAuthCardsAsync(oauthClient, OAuthClientConfig.EmulateOAuthCards));
                return oauthClient;
            }

            return new OAuthClient(new Uri(OAuthClientConfig.OAuthEndpoint), connectorClient.Credentials);
        }

        /// <summary>
        /// Creates the connector client asynchronous.
        /// </summary>
        /// <param name="serviceUrl">The service URL.</param>
        /// <param name="claimsIdentity">The claims identity.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>ConnectorClient instance.</returns>
        /// <exception cref="NotSupportedException">ClaimsIdentity cannot be null. Pass Anonymous ClaimsIdentity if authentication is turned off.</exception>
        private async Task<IConnectorClient> CreateConnectorClientAsync(string serviceUrl, ClaimsIdentity claimsIdentity, CancellationToken cancellationToken)
        {
            if (claimsIdentity == null)
            {
                throw new NotSupportedException("ClaimsIdentity cannot be null. Pass Anonymous ClaimsIdentity if authentication is turned off.");
            }

            // For requests from channel App Id is in Audience claim of JWT token. For emulator it is in AppId claim. For
            // unauthenticated requests we have anonymous identity provided auth is disabled.
            // For Activities coming from Emulator AppId claim contains the Bot's AAD AppId.
            var botAppIdClaim = claimsIdentity.Claims?.SingleOrDefault(claim => claim.Type == AuthenticationConstants.AudienceClaim)
                    ??
                claimsIdentity.Claims?.SingleOrDefault(claim => claim.Type == AuthenticationConstants.AppIdClaim);

            // For anonymous requests (requests with no header) appId is not set in claims.
            if (botAppIdClaim != null)
            {
                string botId = botAppIdClaim.Value;
                var appCredentials = await GetAppCredentialsAsync(botId, cancellationToken).ConfigureAwait(false);
                return CreateConnectorClient(serviceUrl, appCredentials);
            }
            else
            {
                return CreateConnectorClient(serviceUrl);
            }
        }

        /// <summary>
        /// Creates the connector client.
        /// </summary>
        /// <param name="serviceUrl">The service URL.</param>
        /// <param name="appCredentials">The application credentials for the bot.</param>
        /// <returns>Connector client instance.</returns>
        private IConnectorClient CreateConnectorClient(string serviceUrl, AppCredentials appCredentials = null)
        {
            string clientKey = $"{serviceUrl}{appCredentials?.MicrosoftAppId ?? string.Empty}";

            return _connectorClients.GetOrAdd(clientKey, (key) =>
            {
                ConnectorClient connectorClient;
                if (appCredentials != null)
                {
                    connectorClient = new ConnectorClient(new Uri(serviceUrl), appCredentials, customHttpClient: _httpClient);
                }
                else
                {
                    var emptyCredentials = (_channelProvider != null && _channelProvider.IsGovernment()) ?
                        MicrosoftGovernmentAppCredentials.Empty :
                        MicrosoftAppCredentials.Empty;
                    connectorClient = new ConnectorClient(new Uri(serviceUrl), emptyCredentials, customHttpClient: _httpClient);
                }

                if (_connectorClientRetryPolicy != null)
                {
                    connectorClient.SetRetryPolicy(_connectorClientRetryPolicy);
                }

                return connectorClient;
            });
        }

        /// <summary>
        /// Gets the application credentials. App Credentials are cached so as to ensure we are not refreshing
        /// token every time.
        /// </summary>
        /// <param name="appId">The application identifier (AAD Id for the bot).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>App credentials.</returns>
        private async Task<AppCredentials> GetAppCredentialsAsync(string appId, CancellationToken cancellationToken)
        {
            if (appId == null)
            {
                return MicrosoftAppCredentials.Empty;
            }

            if (_appCredentialMap.TryGetValue(appId, out var appCredentials))
            {
                return appCredentials;
            }

            // If app credentials were provided, use them as they are the preferred choice moving forward
            if (_appCredentials != null)
            {
                _appCredentialMap[appId] = _appCredentials;
                return _appCredentials;
            }

            // NOTE: we can't do async operations inside of a AddOrUpdate, so we split access pattern
            string appPassword = await _credentialProvider.GetAppPasswordAsync(appId).ConfigureAwait(false);
            appCredentials = (_channelProvider != null && _channelProvider.IsGovernment()) ?
                new MicrosoftGovernmentAppCredentials(appId, appPassword, _httpClient, _logger) :
                new MicrosoftAppCredentials(appId, appPassword, _httpClient, _logger);
            _appCredentialMap[appId] = appCredentials;
            return appCredentials;
        }

        /// <summary>
        /// Middleware to assign tenantId from channelData to Conversation.TenantId.
        /// </summary>
        /// <description>
        /// MS Teams currently sends the tenant ID in channelData and the correct behavior is to expose this value in Activity.Conversation.TenantId.
        /// This code copies the tenant ID from channelData to Activity.Conversation.TenantId.
        /// Once MS Teams sends the tenantId in the Conversation property, this middleware can be removed.
        /// </description>
        internal class TenantIdWorkaroundForTeamsMiddleware : IMiddleware
        {
            public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
            {
                if (Channels.Msteams.Equals(turnContext.Activity.ChannelId, StringComparison.InvariantCultureIgnoreCase) && turnContext.Activity.Conversation != null && string.IsNullOrEmpty(turnContext.Activity.Conversation.TenantId))
                {
                    var teamsChannelData = JObject.FromObject(turnContext.Activity.ChannelData);
                    if (teamsChannelData["tenant"]?["id"] != null)
                    {
                        turnContext.Activity.Conversation.TenantId = teamsChannelData["tenant"]["id"].ToString();
                    }
                }

                await next(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
