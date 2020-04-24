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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
    /// <seealso cref="IMiddleware"/>h
    public class BotFrameworkAdapter : BotAdapter, IAdapterIntegration, IExtendedUserTokenProvider
    {
        internal const string InvokeResponseKey = "BotFrameworkAdapter.InvokeResponse";

        private static readonly HttpClient DefaultHttpClient = new HttpClient();

        private readonly HttpClient _httpClient;
        private readonly RetryPolicy _connectorClientRetryPolicy;
        private readonly AppCredentials _appCredentials;
        private readonly AuthenticationConfiguration _authConfiguration;

        // Cache for appCredentials to speed up token acquisition (a token is not requested unless is expired)
        // AppCredentials are cached using appId + skillId (this last parameter is only used if the app credentials are used to call a skill)
        private readonly ConcurrentDictionary<string, AppCredentials> _appCredentialMap = new ConcurrentDictionary<string, AppCredentials>();

        // There is a significant boost in throughput if we reuse a connectorClient
        // _connectorClients is a cache using [serviceUrl + appId].
        private readonly ConcurrentDictionary<string, ConnectorClient> _connectorClients = new ConcurrentDictionary<string, ConnectorClient>();

        // Cache for OAuthClient to speed up OAuth operations
        // _oAuthClients is a cache using [appId + oAuthCredentialAppId]
        private readonly ConcurrentDictionary<string, OAuthClient> _oAuthClients = new ConcurrentDictionary<string, OAuthClient>();

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
            CredentialProvider = credentialProvider ?? throw new ArgumentNullException(nameof(credentialProvider));
            ChannelProvider = channelProvider;
            _httpClient = customHttpClient ?? DefaultHttpClient;
            _connectorClientRetryPolicy = connectorClientRetryPolicy;
            Logger = logger ?? NullLogger.Instance;
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
        /// <exception cref="ArgumentNullException">throw ArgumentNullException.</exception>
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
            CredentialProvider = new SimpleCredentialProvider(credentials.MicrosoftAppId, string.Empty);
            this.ChannelProvider = channelProvider;
            _httpClient = customHttpClient ?? DefaultHttpClient;
            _connectorClientRetryPolicy = connectorClientRetryPolicy;
            Logger = logger ?? NullLogger.Instance;
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
        /// Gets the credential provider for this adapter.
        /// </summary>
        /// <value>
        /// The credential provider for this adapter.
        /// </value>
        protected ICredentialProvider CredentialProvider { get; private set; }

        /// <summary>
        /// Gets the channel provider for this adapter.
        /// </summary>
        /// <value>
        /// The channel provider for this adapter.
        /// </value>
        protected IChannelProvider ChannelProvider { get; private set; }

        /// <summary>
        /// Gets the logger for this adapter.
        /// </summary>
        /// <value>
        /// The logger for this adapter.
        /// </value>
        protected ILogger Logger { get; private set; }

        /// <summary>
        /// Gets the map of applications to <see cref="AppCredentials"/> for this adapter.
        /// </summary>
        /// <value>
        /// The map of applications to <see cref="AppCredentials"/> for this adapter.
        /// </value>
        protected ConcurrentDictionary<string, AppCredentials> AppCredentialMap { get => _appCredentialMap; }

        /// <summary>
        /// Gets the custom <see cref="HttpClient"/> for this adapter if specified.
        /// </summary>
        /// <value>
        /// The custom <see cref="HttpClient"/> for this adapter if specified.
        /// </value>
        protected HttpClient HttpClient { get => _httpClient; }

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
        /// <item><description><see cref="IIdentity"/> (key = "BotIdentity"), a claims claimsIdentity for the bot.
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

            Logger.LogInformation($"Sending proactive message.  botAppId: {botAppId}");

            // Hand craft Claims Identity.
            var claimsIdentity = new ClaimsIdentity(new List<Claim>
            {
                // Adding claims for both Emulator and Channel.
                new Claim(AuthenticationConstants.AudienceClaim, botAppId),
                new Claim(AuthenticationConstants.AppIdClaim, botAppId),
            });

            var audience = GetBotFrameworkOAuthScope();

            await ContinueConversationAsync(claimsIdentity, reference, audience, callback, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a proactive message from the bot to a conversation.
        /// </summary>
        /// <param name="claimsIdentity">A <see cref="ClaimsIdentity"/> for the conversation.</param>
        /// <param name="reference">A reference to the conversation to continue.</param>
        /// <param name="callback">The method to call for the resulting bot turn.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>Call this method to proactively send a message to a conversation.
        /// Most _channels require a user to initialize a conversation with a bot
        /// before the bot can send activities to the user.
        /// <para>This method registers the following services for the turn.<list type="bullet">
        /// <item><description><see cref="IIdentity"/> (key = "BotIdentity"), a claims claimsIdentity for the bot.
        /// </description></item>
        /// <item><description><see cref="IConnectorClient"/>, the channel connector client to use this turn.
        /// </description></item>
        /// </list></para>
        /// </remarks>
        /// <seealso cref="ProcessActivityAsync(string, Activity, BotCallbackHandler, CancellationToken)"/>
        /// <seealso cref="BotAdapter.RunPipelineAsync(ITurnContext, BotCallbackHandler, CancellationToken)"/>
        public override async Task ContinueConversationAsync(ClaimsIdentity claimsIdentity, ConversationReference reference, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            var audience = GetBotFrameworkOAuthScope();

            await ContinueConversationAsync(claimsIdentity, reference, audience, callback, cancellationToken).ConfigureAwait(false);
        }

        public override async Task ContinueConversationAsync(ClaimsIdentity claimsIdentity, ConversationReference reference, string audience, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            if (claimsIdentity == null)
            {
                throw new ArgumentNullException(nameof(claimsIdentity));
            }

            if (reference == null)
            {
                throw new ArgumentNullException(nameof(reference));
            }

            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            if (string.IsNullOrWhiteSpace(audience))
            {
                throw new ArgumentNullException($"{nameof(audience)} cannot be null or white space.");
            }

            // Reusing the code from the above override, ContinueConversationAsync()
            using (var context = new TurnContext(this, reference.GetContinuationActivity()))
            {
                context.TurnState.Add<IIdentity>(BotIdentityKey, claimsIdentity);
                context.TurnState.Add<BotCallbackHandler>(callback);

                // Add audience to TurnContext.TurnState
                context.TurnState.Add<string>(OAuthScopeKey, audience);

                var appIdFromClaims = JwtTokenValidation.GetAppIdFromClaims(claimsIdentity.Claims);

                // If we receive a valid app id in the incoming token claims, add the 
                // channel service URL to the trusted services list so we can send messages back.
                // the service URL for skills is trusted because it is applied by the SkillHandler based on the original request
                // received by the root bot
                if (!string.IsNullOrEmpty(appIdFromClaims))
                {
                    var isValidApp = await CredentialProvider.IsValidAppIdAsync(appIdFromClaims).ConfigureAwait(false);

                    if (isValidApp)
                    {
                        AppCredentials.TrustServiceUrl(reference.ServiceUrl);
                    }
                }

                var connectorClient = await CreateConnectorClientAsync(reference.ServiceUrl, claimsIdentity, audience, cancellationToken).ConfigureAwait(false);
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
        /// <item><see cref="IIdentity"/> (key = "BotIdentity"), a claims claimsIdentity for the bot.</item>
        /// <item><see cref="IConnectorClient"/>, the channel connector client to use this turn.</item>
        /// </list></para>
        /// </remarks>
        /// <seealso cref="ContinueConversationAsync(string, ConversationReference, BotCallbackHandler, CancellationToken)"/>
        /// <seealso cref="BotAdapter.RunPipelineAsync(ITurnContext, BotCallbackHandler, CancellationToken)"/>
        public async Task<InvokeResponse> ProcessActivityAsync(string authHeader, Activity activity, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            BotAssert.ActivityNotNull(activity);

            var claimsIdentity = await JwtTokenValidation.AuthenticateRequest(activity, authHeader, CredentialProvider, ChannelProvider, _authConfiguration, _httpClient).ConfigureAwait(false);
            return await ProcessActivityAsync(claimsIdentity, activity, callback, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a turn context and runs the middleware pipeline for an incoming activity.
        /// </summary>
        /// <param name="claimsIdentity">A <see cref="ClaimsIdentity"/> for the request.</param>
        /// <param name="activity">The incoming activity.</param>
        /// <param name="callback">The code to run at the end of the adapter's middleware pipeline.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public override async Task<InvokeResponse> ProcessActivityAsync(ClaimsIdentity claimsIdentity, Activity activity, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            BotAssert.ActivityNotNull(activity);

            Logger.LogInformation($"Received an incoming activity.  ActivityId: {activity.Id}");

            using (var context = new TurnContext(this, activity))
            {
                activity.CallerId = await GenerateCallerIdAsync(claimsIdentity).ConfigureAwait(false);

                context.TurnState.Add<IIdentity>(BotIdentityKey, claimsIdentity);

                // The OAuthScope is also stored on the TurnState to get the correct AppCredentials if fetching a token is required.
                var scope = SkillValidation.IsSkillClaim(claimsIdentity.Claims) ? JwtTokenValidation.GetAppIdFromClaims(claimsIdentity.Claims) : GetBotFrameworkOAuthScope();
                context.TurnState.Add(OAuthScopeKey, scope);
                var connectorClient = await CreateConnectorClientAsync(activity.ServiceUrl, claimsIdentity, scope, cancellationToken).ConfigureAwait(false);
                context.TurnState.Add(connectorClient);

                context.TurnState.Add(callback);

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

                    return (InvokeResponse)activityInvokeResponse.Value;
                }
                else if (context.Activity.DeliveryMode == DeliveryModes.ExpectReplies)
                {
                    return new InvokeResponse { Status = (int)HttpStatusCode.OK, Body = new ExpectedReplies(context.BufferedReplyActivities) };
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

                // Clients and bots SHOULD NOT include an id field in activities they generate.
                // ref: https://github.com/microsoft/botframework-sdk/blob/master/specs/botframework-activity/botframework-activity.md#id
                activity.Id = null;
                var response = default(ResourceResponse);

                Logger.LogInformation($"Sending activity.  ReplyToId: {activity.ReplyToId}");

                if (activity.Type == ActivityTypesEx.Delay)
                {
                    // The Activity Schema doesn't have a delay type built in, so it's simulated
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
                else
                {
                    if (CanProcessOutgoingActivity(activity))
                    {
                        // In cases where implementations of ProcessOutgoingActivityAsync do not fetch a bot token
                        // we want to populate it here in order to make sure credentials are accessible and do not expire.
                        try
                        {
                            var appId = GetBotAppId(turnContext);
                            
                            var oAuthScope = turnContext.TurnState.Get<string>(OAuthScopeKey);
                            _ = (await GetAppCredentialsAsync(appId, oAuthScope).ConfigureAwait(false)).GetTokenAsync();
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError("Failed to fetch token before processing outgoing activity. " + ex.Message);
                        }

                        response = await ProcessOutgoingActivityAsync(turnContext, activity, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(activity.ReplyToId))
                        {
                            var connectorClient = turnContext.TurnState.Get<IConnectorClient>();
                            response = await connectorClient.Conversations.ReplyToActivityAsync(activity, cancellationToken).ConfigureAwait(false);
                        }
                        else
                        {
                            var connectorClient = turnContext.TurnState.Get<IConnectorClient>();
                            response = await connectorClient.Conversations.SendToConversationAsync(activity, cancellationToken).ConfigureAwait(false);
                        }
                    }
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
        public virtual async Task DeleteConversationMemberAsync(ITurnContext turnContext, string memberId, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Conversation == null)
            {
                throw new ArgumentNullException($"{nameof(BotFrameworkAdapter)}.{nameof(DeleteConversationMemberAsync)}(): missing conversation");
            }

            if (string.IsNullOrWhiteSpace(turnContext.Activity.Conversation.Id))
            {
                throw new ArgumentNullException($"{nameof(BotFrameworkAdapter)}.{nameof(DeleteConversationMemberAsync)}(): missing conversation.id");
            }

            var connectorClient = turnContext.TurnState.Get<IConnectorClient>();

            var conversationId = turnContext.Activity.Conversation.Id;

            await connectorClient.Conversations.DeleteConversationMemberAsync(conversationId, memberId, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Lists the members of a given activity.
        /// </summary>
        /// <param name="turnContext">The context object for the turn.</param>
        /// <param name="activityId">(Optional) Activity ID to enumerate. If not specified the current activities ID will be used.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of Members of the activity.</returns>
        public virtual async Task<IList<ChannelAccount>> GetActivityMembersAsync(ITurnContext turnContext, string activityId, CancellationToken cancellationToken)
        {
            // If no activity was passed in, use the current activity.
            if (activityId == null)
            {
                activityId = turnContext.Activity.Id;
            }

            if (turnContext.Activity.Conversation == null)
            {
                throw new ArgumentNullException($"{nameof(BotFrameworkAdapter)}.{nameof(GetActivityMembersAsync)}(): missing conversation");
            }

            if (string.IsNullOrWhiteSpace(turnContext.Activity.Conversation.Id))
            {
                throw new ArgumentNullException($"{nameof(BotFrameworkAdapter)}.{nameof(GetActivityMembersAsync)}(): missing conversation.id");
            }

            var connectorClient = turnContext.TurnState.Get<IConnectorClient>();
            var conversationId = turnContext.Activity.Conversation.Id;

            var accounts = await connectorClient.Conversations.GetActivityMembersAsync(conversationId, activityId, cancellationToken).ConfigureAwait(false);

            return accounts;
        }

        /// <summary>
        /// Lists the members of the current conversation.
        /// </summary>
        /// <param name="turnContext">The context object for the turn.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of Members of the current conversation.</returns>
        public virtual async Task<IList<ChannelAccount>> GetConversationMembersAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Conversation == null)
            {
                throw new ArgumentNullException($"{nameof(BotFrameworkAdapter)}.{nameof(GetConversationMembersAsync)}(): missing conversation");
            }

            if (string.IsNullOrWhiteSpace(turnContext.Activity.Conversation.Id))
            {
                throw new ArgumentNullException($"{nameof(BotFrameworkAdapter)}.{nameof(GetConversationMembersAsync)}(): missing conversation.id");
            }

            var connectorClient = turnContext.TurnState.Get<IConnectorClient>();
            var conversationId = turnContext.Activity.Conversation.Id;

            var accounts = await connectorClient.Conversations.GetConversationMembersAsync(conversationId, cancellationToken).ConfigureAwait(false);
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
        public virtual async Task<ConversationsResult> GetConversationsAsync(ITurnContext turnContext, string continuationToken, CancellationToken cancellationToken)
        {
            var connectorClient = turnContext.TurnState.Get<IConnectorClient>();
            var results = await connectorClient.Conversations.GetConversationsAsync(continuationToken, cancellationToken).ConfigureAwait(false);
            return results;
        }

        /// <summary>
        /// Attempts to retrieve the token for a user that's in a login flow, using customized AppCredentials.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="oAuthAppCredentials">AppCredentials for OAuth.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="magicCode">(Optional) Optional user entered code to validate.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Token Response.</returns>
        public virtual async Task<TokenResponse> GetUserTokenAsync(ITurnContext turnContext, AppCredentials oAuthAppCredentials, string connectionName, string magicCode, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);
            if (turnContext.Activity.From == null || string.IsNullOrWhiteSpace(turnContext.Activity.From.Id))
            {
                throw new ArgumentNullException($"{nameof(BotFrameworkAdapter)}.{nameof(GetUserTokenAsync)}(): missing from or from.id");
            }

            if (string.IsNullOrWhiteSpace(connectionName))
            {
                throw new ArgumentNullException(nameof(connectionName));
            }

            var client = await CreateOAuthApiClientAsync(turnContext, oAuthAppCredentials).ConfigureAwait(false);
            return await client.UserToken.GetTokenAsync(turnContext.Activity.From.Id, connectionName, turnContext.Activity.ChannelId, magicCode, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Attempts to retrieve the token for a user that's in a login flow, using the bot's AppCredentials.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="magicCode">(Optional) Optional user entered code to validate.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Token Response.</returns>
        public virtual async Task<TokenResponse> GetUserTokenAsync(ITurnContext turnContext, string connectionName, string magicCode, CancellationToken cancellationToken = default)
        {
            return await GetUserTokenAsync(turnContext, null, connectionName, magicCode, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the raw signin link to be sent to the user for signin for a connection name, using customized AppCredentials.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="oAuthAppCredentials">AppCredentials for OAuth.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task completes successfully, the result contains the raw signin link.</remarks>
        public virtual async Task<string> GetOauthSignInLinkAsync(ITurnContext turnContext, AppCredentials oAuthAppCredentials, string connectionName, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);
            if (string.IsNullOrWhiteSpace(connectionName))
            {
                throw new ArgumentNullException(nameof(connectionName));
            }

            var activity = turnContext.Activity;
            var appId = GetBotAppId(turnContext);
            var tokenExchangeState = new TokenExchangeState()
            {
                ConnectionName = connectionName,
                Conversation = new ConversationReference()
                {
                    ActivityId = activity.Id,
                    Bot = activity.Recipient,       // Activity is from the user to the bot
                    ChannelId = activity.ChannelId,
                    Conversation = activity.Conversation,
                    Locale = activity.Locale,
                    ServiceUrl = activity.ServiceUrl,
                    User = activity.From,
                },
                RelatesTo = activity.RelatesTo,
                MsAppId = appId,
            };

            var serializedState = JsonConvert.SerializeObject(tokenExchangeState);
            var encodedState = Encoding.UTF8.GetBytes(serializedState);
            var state = Convert.ToBase64String(encodedState);

            var client = await CreateOAuthApiClientAsync(turnContext, oAuthAppCredentials).ConfigureAwait(false);
            return await client.BotSignIn.GetSignInUrlAsync(state, null, null, null, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the raw signin link to be sent to the user for signin for a connection name, using the bot's AppCredentials.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task completes successfully, the result contains the raw signin link.</remarks>
        public virtual async Task<string> GetOauthSignInLinkAsync(ITurnContext turnContext, string connectionName, CancellationToken cancellationToken = default)
        {
            return await GetOauthSignInLinkAsync(turnContext, null, connectionName, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the raw signin link to be sent to the user for signin for a connection name, using customized AppCredentials.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="oAuthAppCredentials">AppCredentials for OAuth.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="userId">The user id that will be associated with the token.</param>
        /// <param name="finalRedirect">The final URL that the OAuth flow will redirect to.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task completes successfully, the result contains the raw signin link.</remarks>
        public virtual async Task<string> GetOauthSignInLinkAsync(ITurnContext turnContext, AppCredentials oAuthAppCredentials, string connectionName, string userId, string finalRedirect = null, CancellationToken cancellationToken = default)
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

            var activity = turnContext.Activity;

            var appId = GetBotAppId(turnContext);
            var tokenExchangeState = new TokenExchangeState()
            {
                ConnectionName = connectionName,
                Conversation = new ConversationReference()
                {
                    ActivityId = activity.Id,
                    Bot = activity.Recipient,       // Activity is from the user to the bot
                    ChannelId = activity.ChannelId,
                    Conversation = activity.Conversation,
                    Locale = activity.Locale,
                    ServiceUrl = activity.ServiceUrl,
                    User = activity.From,
                },
                RelatesTo = activity.RelatesTo,
                MsAppId = appId,
            };

            var serializedState = JsonConvert.SerializeObject(tokenExchangeState);
            var encodedState = Encoding.UTF8.GetBytes(serializedState);
            var state = Convert.ToBase64String(encodedState);

            var client = await CreateOAuthApiClientAsync(turnContext, oAuthAppCredentials).ConfigureAwait(false);
            return await client.BotSignIn.GetSignInUrlAsync(state, null, null, finalRedirect, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the raw signin link to be sent to the user for signin for a connection name, using the bot's AppCredentials.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="userId">The user id that will be associated with the token.</param>
        /// <param name="finalRedirect">The final URL that the OAuth flow will redirect to.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task completes successfully, the result contains the raw signin link.</remarks>
        public virtual async Task<string> GetOauthSignInLinkAsync(ITurnContext turnContext, string connectionName, string userId, string finalRedirect = null, CancellationToken cancellationToken = default)
        {
            return await GetOauthSignInLinkAsync(turnContext, null, connectionName, userId, finalRedirect, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Signs the user out with the token server, using customized AppCredentials.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="oAuthAppCredentials">AppCredentials for OAuth.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="userId">User id of user to sign out.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public virtual async Task SignOutUserAsync(ITurnContext turnContext, AppCredentials oAuthAppCredentials, string connectionName = null, string userId = null, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(turnContext);

            if (string.IsNullOrEmpty(userId))
            {
                userId = turnContext.Activity?.From?.Id;
            }

            var client = await CreateOAuthApiClientAsync(turnContext, oAuthAppCredentials).ConfigureAwait(false);
            await client.UserToken.SignOutAsync(userId, connectionName, turnContext.Activity?.ChannelId, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Signs the user out with the token server, using the bot's AppCredentials.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="userId">User id of user to sign out.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public virtual async Task SignOutUserAsync(ITurnContext turnContext, string connectionName = null, string userId = null, CancellationToken cancellationToken = default)
        {
            await SignOutUserAsync(turnContext, null, connectionName, userId, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves the token status for each configured connection for the given user, using customized AppCredentials.
        /// </summary>
        /// <param name="context">Context for the current turn of conversation with the user.</param>
        /// <param name="oAuthAppCredentials">AppCredentials for OAuth.</param>
        /// <param name="userId">The user Id for which token status is retrieved.</param>
        /// <param name="includeFilter">Optional comma separated list of connection's to include. Blank will return token status for all configured connections.</param>
        /// <param name="cancellationToken">The async operation cancellation token.</param>
        /// <returns>Array of TokenStatus.</returns>
        public virtual async Task<TokenStatus[]> GetTokenStatusAsync(ITurnContext context, AppCredentials oAuthAppCredentials, string userId, string includeFilter = null, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(context);

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            var client = await CreateOAuthApiClientAsync(context, oAuthAppCredentials).ConfigureAwait(false);
            var result = await client.UserToken.GetTokenStatusAsync(userId, context.Activity?.ChannelId, includeFilter, cancellationToken).ConfigureAwait(false);
            return result?.ToArray();
        }

        /// <summary>
        /// Retrieves the token status for each configured connection for the given user, using the bot's AppCredentials.
        /// </summary>
        /// <param name="context">Context for the current turn of conversation with the user.</param>
        /// <param name="userId">The user Id for which token status is retrieved.</param>
        /// <param name="includeFilter">Optional comma separated list of connection's to include. Blank will return token status for all configured connections.</param>
        /// <param name="cancellationToken">The async operation cancellation token.</param>
        /// <returns>Array of TokenStatus.</returns>
        public virtual async Task<TokenStatus[]> GetTokenStatusAsync(ITurnContext context, string userId, string includeFilter = null, CancellationToken cancellationToken = default)
        {
            return await GetTokenStatusAsync(context, null, userId, includeFilter, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves Azure Active Directory tokens for particular resources on a configured connection, using customized AppCredentials.
        /// </summary>
        /// <param name="context">Context for the current turn of conversation with the user.</param>
        /// <param name="oAuthAppCredentials">AppCredentials for OAuth.</param>
        /// <param name="connectionName">The name of the Azure Active Directory connection configured with this bot.</param>
        /// <param name="resourceUrls">The list of resource URLs to retrieve tokens for.</param>
        /// <param name="userId">The user Id for which tokens are retrieved. If passing in null the userId is taken from the Activity in the ITurnContext.</param>
        /// <param name="cancellationToken">The async operation cancellation token.</param>
        /// <returns>Dictionary of resourceUrl to the corresponding TokenResponse.</returns>
        public virtual async Task<Dictionary<string, TokenResponse>> GetAadTokensAsync(ITurnContext context, AppCredentials oAuthAppCredentials, string connectionName, string[] resourceUrls, string userId = null, CancellationToken cancellationToken = default)
        {
            BotAssert.ContextNotNull(context);

            if (string.IsNullOrWhiteSpace(connectionName))
            {
                throw new ArgumentNullException(nameof(connectionName));
            }

            if (resourceUrls == null)
            {
                throw new ArgumentNullException(nameof(resourceUrls));
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                userId = context.Activity?.From?.Id;
            }

            var client = await CreateOAuthApiClientAsync(context, oAuthAppCredentials).ConfigureAwait(false);
            return (Dictionary<string, TokenResponse>)await client.UserToken.GetAadTokensAsync(userId, connectionName, new AadResourceUrls() { ResourceUrls = resourceUrls?.ToList() }, context.Activity?.ChannelId, cancellationToken).ConfigureAwait(false);
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
        public virtual Task<SignInResource> GetSignInResourceAsync(ITurnContext turnContext, string connectionName, CancellationToken cancellationToken = default(CancellationToken))
        {
            return GetSignInResourceAsync(turnContext, connectionName, turnContext.Activity.From.Id, null, cancellationToken);
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
        public virtual Task<SignInResource> GetSignInResourceAsync(ITurnContext turnContext, string connectionName, string userId, string finalRedirect = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return GetSignInResourceAsync(turnContext, null, connectionName, userId, finalRedirect, cancellationToken);
        }

        /// <summary>
        /// Get the raw signin link to be sent to the user for signin for a connection name.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="oAuthAppCredentials">AppCredentials for OAuth.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="userId">The user id that will be associated with the token.</param>
        /// <param name="finalRedirect">The final URL that the OAuth flow will redirect to.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task completes successfully, the result contains the raw signin link.</remarks>
        public virtual async Task<SignInResource> GetSignInResourceAsync(ITurnContext turnContext, AppCredentials oAuthAppCredentials, string connectionName, string userId, string finalRedirect = null, CancellationToken cancellationToken = default(CancellationToken))
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

            var activity = turnContext.Activity;

            var appId = GetBotAppId(turnContext);
            var tokenExchangeState = new TokenExchangeState()
            {
                ConnectionName = connectionName,
                Conversation = new ConversationReference()
                {
                    ActivityId = activity.Id,
                    Bot = activity.Recipient,       // Activity is from the user to the bot
                    ChannelId = activity.ChannelId,
                    Conversation = activity.Conversation,
                    Locale = activity.Locale,
                    ServiceUrl = activity.ServiceUrl,
                    User = activity.From,
                },
                RelatesTo = activity.RelatesTo,
                MsAppId = appId,
            };

            var serializedState = JsonConvert.SerializeObject(tokenExchangeState);
            var encodedState = Encoding.UTF8.GetBytes(serializedState);
            var state = Convert.ToBase64String(encodedState);

            var client = await CreateOAuthApiClientAsync(turnContext, oAuthAppCredentials).ConfigureAwait(false);
            return await client.GetSignInResourceAsync(state, null, null, finalRedirect, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Performs a token exchange operation such as for single sign-on.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="userId">The user id associated with the token..</param>
        /// <param name="exchangeRequest">The exchange request details, either a token to exchange or a uri to exchange.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>If the task completes, the exchanged token is returned.</returns>
        public virtual Task<TokenResponse> ExchangeTokenAsync(ITurnContext turnContext, string connectionName, string userId, TokenExchangeRequest exchangeRequest, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ExchangeTokenAsync(turnContext, null, connectionName, userId, exchangeRequest, cancellationToken);
        }

        /// <summary>
        /// Performs a token exchange operation such as for single sign-on.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="oAuthAppCredentials">AppCredentials for OAuth.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="userId">The user id associated with the token..</param>
        /// <param name="exchangeRequest">The exchange request details, either a token to exchange or a uri to exchange.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>If the task completes, the exchanged token is returned.</returns>
        public virtual async Task<TokenResponse> ExchangeTokenAsync(ITurnContext turnContext, AppCredentials oAuthAppCredentials, string connectionName, string userId, TokenExchangeRequest exchangeRequest, CancellationToken cancellationToken = default(CancellationToken))
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

            if (exchangeRequest == null)
            {
                throw new ArgumentNullException(nameof(exchangeRequest));
            }

            if (string.IsNullOrWhiteSpace(exchangeRequest.Token) && string.IsNullOrWhiteSpace(exchangeRequest.Uri))
            {
                throw new ArgumentException(nameof(exchangeRequest), "Either a Token or Uri property is required on the TokenExchangeRequest");
            }

            var activity = turnContext.Activity;

            var client = await CreateOAuthApiClientAsync(turnContext, oAuthAppCredentials).ConfigureAwait(false);
            var result = await client.ExchangeAsyncAsync(userId, connectionName, turnContext.Activity.ChannelId, exchangeRequest, cancellationToken).ConfigureAwait(false);

            if (result is ErrorResponse errorResponse)
            {
                throw new InvalidOperationException($"Unable to exchange token: ({errorResponse?.Error?.Code}) {errorResponse?.Error?.Message}");
            }

            if (result is TokenResponse tokenResponse)
            {
                return tokenResponse;
            }
            else
            {
                throw new InvalidOperationException($"ExchangeAsyncAsync returned improper result: {result.GetType()}");
            }
        }

        /// <summary>
        /// Retrieves Azure Active Directory tokens for particular resources on a configured connection, using the bot's AppCredentials.
        /// </summary>
        /// <param name="context">Context for the current turn of conversation with the user.</param>
        /// <param name="connectionName">The name of the Azure Active Directory connection configured with this bot.</param>
        /// <param name="resourceUrls">The list of resource URLs to retrieve tokens for.</param>
        /// <param name="userId">The user Id for which tokens are retrieved. If passing in null the userId is taken from the Activity in the ITurnContext.</param>
        /// <param name="cancellationToken">The async operation cancellation token.</param>
        /// <returns>Dictionary of resourceUrl to the corresponding TokenResponse.</returns>
        public virtual async Task<Dictionary<string, TokenResponse>> GetAadTokensAsync(ITurnContext context, string connectionName, string[] resourceUrls, string userId = null, CancellationToken cancellationToken = default)
        {
            return await GetAadTokensAsync(context, null, connectionName, resourceUrls, userId, cancellationToken).ConfigureAwait(false);
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

            using (var context = new TurnContext(this, (Activity)eventActivity))
            {
                var claimsIdentity = new ClaimsIdentity();
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
                    conversationParameters.ChannelData = new { tenant = new { tenantId } };

                    // Permanent solution is to put tenantId in parameters.tenantId
                    conversationParameters.TenantId = tenantId;
                }

                await CreateConversationAsync(channelId, serviceUrl, credentials, conversationParameters, callback, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Creates an OAuth client for the bot with the credentials.
        /// </summary>
        /// <param name="turnContext">The context object for the current turn.</param>
        /// <param name="oAuthAppCredentials">AppCredentials for OAuth.</param>
        /// <returns>An OAuth client for the bot.</returns>
        protected virtual async Task<OAuthClient> CreateOAuthApiClientAsync(ITurnContext turnContext, AppCredentials oAuthAppCredentials)
        {
            if (!OAuthClientConfig.EmulateOAuthCards &&
                string.Equals(turnContext.Activity.ChannelId, Channels.Emulator, StringComparison.InvariantCultureIgnoreCase) &&
                (await CredentialProvider.IsAuthenticationDisabledAsync().ConfigureAwait(false)))
            {
                OAuthClientConfig.EmulateOAuthCards = true;
            }

            var appId = GetBotAppId(turnContext);

            var clientKey = $"{appId}:{oAuthAppCredentials?.MicrosoftAppId}";
            var oAuthScope = GetBotFrameworkOAuthScope();

            var appCredentials = oAuthAppCredentials ?? await GetAppCredentialsAsync(appId, oAuthScope).ConfigureAwait(false);

            if (!OAuthClientConfig.EmulateOAuthCards &&
                string.Equals(turnContext.Activity.ChannelId, Channels.Emulator, StringComparison.InvariantCultureIgnoreCase) &&
                (await CredentialProvider.IsAuthenticationDisabledAsync().ConfigureAwait(false)))
            {
                OAuthClientConfig.EmulateOAuthCards = true;
            }

            var oAuthClient = _oAuthClients.GetOrAdd(clientKey, (key) =>
            {
                OAuthClient oAuthClientInner;
                if (OAuthClientConfig.EmulateOAuthCards)
                {
                    // do not await task - we want this to run in the background
                    oAuthClientInner = new OAuthClient(new Uri(turnContext.Activity.ServiceUrl), appCredentials);
                    var task = Task.Run(() => OAuthClientConfig.SendEmulateOAuthCardsAsync(oAuthClientInner, OAuthClientConfig.EmulateOAuthCards));
                }
                else
                {
                    oAuthClientInner = new OAuthClient(new Uri(OAuthClientConfig.OAuthEndpoint), appCredentials);
                }

                return oAuthClientInner;
            });

            // adding the oAuthClient into the TurnState
            // TokenResolver.cs will use it get the correct credentials to poll for token for streaming scenario
            if (turnContext.TurnState.Get<OAuthClient>() == null)
            {
                turnContext.TurnState.Add(oAuthClient);
            }

            return oAuthClient;
        }

        /// <summary>
        /// Creates an OAuth client for the bot.
        /// </summary>
        /// <param name="turnContext">The context object for the current turn.</param>
        /// <returns>An OAuth client for the bot.</returns>
        protected virtual async Task<OAuthClient> CreateOAuthApiClientAsync(ITurnContext turnContext)
        {
            return await CreateOAuthApiClientAsync(turnContext, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Opportunity for subclasses to opt in to process an outgoing activity.
        /// </summary>
        /// <remarks>
        /// Subclasses can override ProcessOutgoingActivityAsync. If CanProcessOutgoingActivity returns true, 
        /// ProcessOutgoingActivityAsync will be responsible for sending the outgoing activity.
        /// </remarks>
        /// <param name="activity">The outgoing activity.</param>
        /// <returns>Whether should call ProcessOutgoingActivityAsync to send the outgoing activity.</returns>
        protected virtual bool CanProcessOutgoingActivity(Activity activity)
        {
            return false;
        }

        /// <summary>
        /// Custom logic to send an outgoing activity. Subclasses can override this method along with CanProcessOutgoingActivity
        /// to have custom logic to process the outgoing activity.
        /// </summary>
        /// <param name="turnContext">The context object for the turn.</param>
        /// <param name="activity">The activity to be processed.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of processing the activity.</returns>
        protected virtual Task<ResourceResponse> ProcessOutgoingActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Logic to build an <see cref="AppCredentials"/> object to be used to acquire tokens
        /// for this HttpClient.
        /// </summary>
        /// <param name="appId">The application id.</param>
        /// <param name="oAuthScope">The optional OAuth scope.</param>
        /// <returns>The app credentials to be used to acquire tokens.</returns>
        protected virtual async Task<AppCredentials> BuildCredentialsAsync(string appId, string oAuthScope = null)
        {
            // Get the password from the credential provider
            var appPassword = await CredentialProvider.GetAppPasswordAsync(appId).ConfigureAwait(false);

            // Construct an AppCredentials using the app + password combination. If government, we create a government specific credential.
            return ChannelProvider != null && ChannelProvider.IsGovernment() ? new MicrosoftGovernmentAppCredentials(appId, appPassword, HttpClient, Logger, oAuthScope) : new MicrosoftAppCredentials(appId, appPassword, HttpClient, Logger, oAuthScope);
        }

        /// <summary>
        /// Generates the CallerId property for the activity based on
        /// https://github.com/microsoft/botframework-obi/blob/master/protocols/botframework-activity/botframework-activity.md#appendix-v---caller-id-values.
        /// </summary>
        private async Task<string> GenerateCallerIdAsync(ClaimsIdentity claimsIdentity)
        {
            // Is the bot accepting all incoming messages?
            var isAuthDisabled = await CredentialProvider.IsAuthenticationDisabledAsync().ConfigureAwait(false);
            if (isAuthDisabled) 
            {
                // Return null so that the callerId is cleared.
                return null;
            }
        
            // Is the activity from another bot?
            if (SkillValidation.IsSkillClaim(claimsIdentity.Claims))
            {
                return $"{CallerIdConstants.BotToBotPrefix}{JwtTokenValidation.GetAppIdFromClaims(claimsIdentity.Claims)}";
            }

            // Is the activity from Public Azure?
            if (ChannelProvider == null || ChannelProvider.IsPublicAzure()) 
            {
                return CallerIdConstants.PublicAzureChannel;
            }

            // Is the activity from Azure Gov?
            if (ChannelProvider != null && ChannelProvider.IsGovernment()) 
            {
                return CallerIdConstants.USGovChannel;
            }

            // Return null so that the callerId is cleared.
            return null;
        }

        /// <summary>
        /// Creates the connector client asynchronous.
        /// </summary>
        /// <param name="serviceUrl">The service URL.</param>
        /// <param name="claimsIdentity">The claims claimsIdentity.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>ConnectorClient instance.</returns>
        /// <exception cref="NotSupportedException">ClaimsIdentity cannot be null. Pass Anonymous ClaimsIdentity if authentication is turned off.</exception>
        private async Task<IConnectorClient> CreateConnectorClientAsync(string serviceUrl, ClaimsIdentity claimsIdentity, string audience, CancellationToken cancellationToken = default)
        {
            if (claimsIdentity == null)
            {
                throw new NotSupportedException("ClaimsIdentity cannot be null. Pass Anonymous ClaimsIdentity if authentication is turned off.");
            }

            // For requests from channel App Id is in Audience claim of JWT token. For emulator it is in AppId claim. For
            // unauthenticated requests we have anonymous claimsIdentity provided auth is disabled.
            // For Activities coming from Emulator AppId claim contains the Bot's AAD AppId.
            var botAppIdClaim = claimsIdentity.Claims?.SingleOrDefault(claim => claim.Type == AuthenticationConstants.AudienceClaim);
            if (botAppIdClaim == null)
            {
                botAppIdClaim = claimsIdentity.Claims?.SingleOrDefault(claim => claim.Type == AuthenticationConstants.AppIdClaim);
            }

            // For anonymous requests (requests with no header) appId is not set in claims.
            AppCredentials appCredentials = null;
            if (botAppIdClaim != null)
            {
                var botId = botAppIdClaim.Value;
                var scope = audience;

                if (string.IsNullOrWhiteSpace(audience))
                {
                    // The skill connector has the target skill in the OAuthScope.
                    scope = SkillValidation.IsSkillClaim(claimsIdentity.Claims) ?
                        JwtTokenValidation.GetAppIdFromClaims(claimsIdentity.Claims) :
                        GetBotFrameworkOAuthScope();
                }

                appCredentials = await GetAppCredentialsAsync(botId, scope, cancellationToken).ConfigureAwait(false);
            }

            return CreateConnectorClient(serviceUrl, appCredentials);
        }

        /// <summary>
        /// Creates the connector client.
        /// </summary>
        /// <param name="serviceUrl">The service URL.</param>
        /// <param name="appCredentials">The application credentials for the bot.</param>
        /// <returns>Connector client instance.</returns>
        private IConnectorClient CreateConnectorClient(string serviceUrl, AppCredentials appCredentials = null)
        {
            // As multiple bots can listen on a single serviceUrl, the clientKey also includes the OAuthScope.
            var clientKey = $"{serviceUrl}{appCredentials?.MicrosoftAppId}:{appCredentials?.OAuthScope}";

            return _connectorClients.GetOrAdd(clientKey, (key) =>
            {
                ConnectorClient connectorClient;
                if (appCredentials != null)
                {
                    connectorClient = new ConnectorClient(new Uri(serviceUrl), appCredentials, customHttpClient: _httpClient);
                }
                else
                {
                    var emptyCredentials = (ChannelProvider != null && ChannelProvider.IsGovernment()) ?
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
        /// Gets the application credentials. App credentials are cached to avoid refreshing the
        /// token each time.
        /// </summary>
        /// <param name="appId">The application identifier (AAD ID for the bot).</param>
        /// <param name="oAuthScope">The scope for the token. Skills use the skill's app ID. </param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>App credentials.</returns>
        private async Task<AppCredentials> GetAppCredentialsAsync(string appId, string oAuthScope, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(appId))
            {
                return MicrosoftAppCredentials.Empty;
            }

            var cacheKey = $"{appId}{oAuthScope}";
            if (_appCredentialMap.TryGetValue(cacheKey, out var appCredentials))
            {
                return appCredentials;
            }

            // If app credentials were provided, use them as they are the preferred choice moving forward
            if (_appCredentials != null)
            {
                // Cache the credentials for later use
                _appCredentialMap[cacheKey] = _appCredentials;
                return _appCredentials;
            }

            // Credentials not found in cache, build them
            appCredentials = await BuildCredentialsAsync(appId, oAuthScope).ConfigureAwait(false);

            // Cache the credentials for later use
            _appCredentialMap[cacheKey] = appCredentials;
            return appCredentials;
        }

        /// <summary>
        /// Gets the AppId of the Bot out of the TurnState.
        /// </summary>
        /// <param name="turnContext">The context object for the turn.</param>
        /// <returns>Bot's AppId.</returns>
        private string GetBotAppId(ITurnContext turnContext)
        {
            var botIdentity = (ClaimsIdentity)turnContext.TurnState.Get<IIdentity>(BotIdentityKey);
            if (botIdentity == null)
            {
                throw new InvalidOperationException("An IIdentity is required in TurnState for this operation.");
            }

            var appId = botIdentity.Claims.FirstOrDefault(claim => claim.Type == AuthenticationConstants.AudienceClaim)?.Value;
            if (string.IsNullOrWhiteSpace(appId))
            {
                throw new InvalidOperationException("Unable to get the bot AppId from the audience claim.");
            }

            return appId;
        }

        /// <summary>
        /// This method returns the correct Bot Framework OAuthScope for AppCredentials.
        /// </summary>
        private string GetBotFrameworkOAuthScope()
        {
            return ChannelProvider != null && ChannelProvider.IsGovernment() ?
                GovernmentAuthenticationConstants.ToChannelFromBotOAuthScope :
                AuthenticationConstants.ToChannelFromBotOAuthScope;
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
            public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
            {
                if (Channels.Msteams.Equals(turnContext.Activity.ChannelId, StringComparison.InvariantCultureIgnoreCase) && turnContext.Activity.Conversation != null && string.IsNullOrEmpty(turnContext.Activity.Conversation.TenantId) && turnContext.Activity.ChannelData != null)
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
