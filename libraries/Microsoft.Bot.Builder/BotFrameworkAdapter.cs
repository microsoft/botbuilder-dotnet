// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Rest.TransientFaultHandling;

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
    public class BotFrameworkAdapter : BotAdapter
    {
        private const string InvokeReponseKey = "BotFrameworkAdapter.InvokeResponse";
        private const string BotIdentityKey = "BotIdentity";

        private static readonly HttpClient DefaultHttpClient = new HttpClient();
        private readonly ICredentialProvider _credentialProvider;
        private readonly HttpClient _httpClient;
        private readonly RetryPolicy _connectorClientRetryPolicy;
        private Dictionary<string, MicrosoftAppCredentials> _appCredentialMap = new Dictionary<string, MicrosoftAppCredentials>();
        private bool _isEmulatingOAuthCards = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="BotFrameworkAdapter"/> class,
        /// using a credential provider.
        /// </summary>
        /// <param name="credentialProvider">The credential provider.</param>
        /// <param name="connectorClientRetryPolicy">Retry policy for retrying HTTP operations.</param>
        /// <param name="customHttpClient">The HTTP client.</param>
        /// <param name="middleware">The middleware to initially add to the adapter.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="credentialProvider"/> is <c>null</c>.</exception>
        /// <remarks>Use a <see cref="MiddlewareSet"/> object to add multiple middleware
        /// components in the conustructor. Use the <see cref="Use(IMiddleware)"/> method to
        /// add additional middleware to the adapter after construction.
        /// </remarks>
        public BotFrameworkAdapter(ICredentialProvider credentialProvider, RetryPolicy connectorClientRetryPolicy = null, HttpClient customHttpClient = null, IMiddleware middleware = null)
        {
            _credentialProvider = credentialProvider ?? throw new ArgumentNullException(nameof(credentialProvider));
            _httpClient = customHttpClient ?? DefaultHttpClient;
            _connectorClientRetryPolicy = connectorClientRetryPolicy;

            if (middleware != null)
            {
                Use(middleware);
            }
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
        /// Most _channels require a user to initaiate a conversation with a bot
        /// before the bot can send activities to the user.
        /// <para>This method registers the following services for the turn.<list type="bullet">
        /// <item><see cref="IIdentity"/> (key = "BotIdentity"), a claims identity for the bot.</item>
        /// <item><see cref="IConnectorClient"/>, the channel connector client to use this turn.</item>
        /// </list></para>
        /// <para>
        /// This overload differers from the Node implementation by requiring the BotId to be
        /// passed in. The .Net code allows multiple bots to be hosted in a single adapter which
        /// isn't something supported by Node.
        /// </para>
        /// </remarks>
        /// <seealso cref="ProcessActivityAsync(string, Activity, Func{ITurnContext, Task}, CancellationToken)"/>
        /// <seealso cref="BotAdapter.RunPipelineAsync(ITurnContext, Func{ITurnContext, Task}, CancellationToken)"/>
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

            using (var context = new TurnContext(this, reference.GetContinuationActivity()))
            {
                // Hand craft Claims Identity.
                var claimsIdentity = new ClaimsIdentity(new List<Claim>
                {
                    // Adding claims for both Emulator and Channel.
                    new Claim(AuthenticationConstants.AudienceClaim, botAppId),
                    new Claim(AuthenticationConstants.AppIdClaim, botAppId),
                });

                context.Services.Add<IIdentity>(BotIdentityKey, claimsIdentity);
                var connectorClient = await CreateConnectorClientAsync(reference.ServiceUrl, claimsIdentity, cancellationToken).ConfigureAwait(false);
                context.Services.Add(connectorClient);
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

            var claimsIdentity = await JwtTokenValidation.AuthenticateRequest(activity, authHeader, _credentialProvider, _httpClient).ConfigureAwait(false);
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

            using (var context = new TurnContext(this, activity))
            {
                context.Services.Add<IIdentity>(BotIdentityKey, identity);

                var connectorClient = await CreateConnectorClientAsync(activity.ServiceUrl, identity, cancellationToken).ConfigureAwait(false);
                context.Services.Add(connectorClient);

                await RunPipelineAsync(context, callback, cancellationToken).ConfigureAwait(false);

                // Handle Invoke scenarios, which deviate from the request/response model in that
                // the Bot will return a specific body and return code.
                if (activity.Type == ActivityTypes.Invoke)
                {
                    Activity invokeResponse = context.Services.Get<Activity>(InvokeReponseKey);
                    if (invokeResponse == null)
                    {
                        // ToDo: Trace Here
                        throw new InvalidOperationException("Bot failed to return a valid 'invokeResponse' activity.");
                    }
                    else
                    {
                        return (InvokeResponse)invokeResponse.Value;
                    }
                }

                // For all non-invoke scenarios, the HTTP layers above don't have to mess
                // withthe Body and return codes.
                return null;
            }
        }

        /// <summary>
        /// Sends activities to the conversation.
        /// </summary>
        /// <param name="context">The context object for the turn.</param>
        /// <param name="activities">The activities to send.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the activities are successfully sent, the task result contains
        /// an array of <see cref="ResourceResponse"/> objects containing the IDs that
        /// the receiving channel assigned to the activities.</remarks>
        /// <seealso cref="ITurnContext.OnSendActivities(SendActivitiesHandler)"/>
        public override async Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext context, Activity[] activities, CancellationToken cancellationToken)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
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
                    context.Services.Add(InvokeReponseKey, activity);

                    // No need to create a response. One will be created below.
                }
                else if (activity.Type == ActivityTypes.Trace && activity.ChannelId != "emulator")
                {
                    // if it is a Trace activity we only send to the channel if it's the emulator.
                }
                else if (!string.IsNullOrWhiteSpace(activity.ReplyToId))
                {
                    var connectorClient = context.Services.Get<IConnectorClient>();
                    response = await connectorClient.Conversations.ReplyToActivityAsync(activity, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    var connectorClient = context.Services.Get<IConnectorClient>();
                    response = await connectorClient.Conversations.SendToConversationAsync(activity, cancellationToken).ConfigureAwait(false);
                }

                // If No response is set, then defult to a "simple" response. This can't really be done
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
        /// <param name="context">The context object for the turn.</param>
        /// <param name="activity">New replacement activity.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the activity is successfully sent, the task result contains
        /// a <see cref="ResourceResponse"/> object containing the ID that the receiving
        /// channel assigned to the activity.
        /// <para>Before calling this, set the ID of the replacement activity to the ID
        /// of the activity to replace.</para></remarks>
        /// <seealso cref="ITurnContext.OnUpdateActivity(UpdateActivityHandler)"/>
        public override async Task<ResourceResponse> UpdateActivityAsync(ITurnContext context, Activity activity, CancellationToken cancellationToken)
        {
            var connectorClient = context.Services.Get<IConnectorClient>();
            return await connectorClient.Conversations.UpdateActivityAsync(activity, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes an existing activity in the conversation.
        /// </summary>
        /// <param name="context">The context object for the turn.</param>
        /// <param name="reference">Conversation reference for the activity to delete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>The <see cref="ConversationReference.ActivityId"/> of the conversation
        /// reference identifies the activity to delete.</remarks>
        /// <seealso cref="ITurnContext.OnDeleteActivity(DeleteActivityHandler)"/>
        public override async Task DeleteActivityAsync(ITurnContext context, ConversationReference reference, CancellationToken cancellationToken)
        {
            var connectorClient = context.Services.Get<IConnectorClient>();
            await connectorClient.Conversations.DeleteActivityAsync(reference.Conversation.Id, reference.ActivityId, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Removes a member from the current conversation.
        /// </summary>
        /// <param name="context">The context object for the turn.</param>
        /// <param name="memberId">The ID of the member to remove from the conversation.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task DeleteConversationMemberAsync(ITurnContext context, string memberId, CancellationToken cancellationToken)
        {
            if (context.Activity.Conversation == null)
            {
                throw new ArgumentNullException("BotFrameworkAdapter.deleteConversationMember(): missing conversation");
            }

            if (string.IsNullOrWhiteSpace(context.Activity.Conversation.Id))
            {
                throw new ArgumentNullException("BotFrameworkAdapter.deleteConversationMember(): missing conversation.id");
            }

            var connectorClient = context.Services.Get<IConnectorClient>();

            string conversationId = context.Activity.Conversation.Id;

            await connectorClient.Conversations.DeleteConversationMemberAsync(conversationId, memberId, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Lists the members of a given activity.
        /// </summary>
        /// <param name="context">The context object for the turn.</param>
        /// <param name="activityId">(Optional) Activity ID to enumerate. If not specified the current activities ID will be used.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of Members of the activity.</returns>
        public async Task<IList<ChannelAccount>> GetActivityMembersAsync(ITurnContext context, string activityId, CancellationToken cancellationToken)
        {
            // If no activity was passed in, use the current activity.
            if (activityId == null)
            {
                activityId = context.Activity.Id;
            }

            if (context.Activity.Conversation == null)
            {
                throw new ArgumentNullException("BotFrameworkAdapter.GetActivityMembers(): missing conversation");
            }

            if (string.IsNullOrWhiteSpace(context.Activity.Conversation.Id))
            {
                throw new ArgumentNullException("BotFrameworkAdapter.GetActivityMembers(): missing conversation.id");
            }

            var connectorClient = context.Services.Get<IConnectorClient>();
            var conversationId = context.Activity.Conversation.Id;

            IList<ChannelAccount> accounts = await connectorClient.Conversations.GetActivityMembersAsync(conversationId, activityId, cancellationToken).ConfigureAwait(false);

            return accounts;
        }

        /// <summary>
        /// Lists the members of the current conversation.
        /// </summary>
        /// <param name="context">The context object for the turn.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of Members of the current conversation.</returns>
        public async Task<IList<ChannelAccount>> GetConversationMembersAsync(ITurnContext context, CancellationToken cancellationToken)
        {
            if (context.Activity.Conversation == null)
            {
                throw new ArgumentNullException("BotFrameworkAdapter.GetActivityMembers(): missing conversation");
            }

            if (string.IsNullOrWhiteSpace(context.Activity.Conversation.Id))
            {
                throw new ArgumentNullException("BotFrameworkAdapter.GetActivityMembers(): missing conversation.id");
            }

            var connectorClient = context.Services.Get<IConnectorClient>();
            var conversationId = context.Activity.Conversation.Id;

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
        /// <param name="continuationToken"></param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task completes successfully, the result contains the members of the current conversation.
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
        /// <param name="context">The context object for the turn.</param>
        /// <param name="continuationToken"></param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task completes successfully, the result contains the members of the current conversation.
        /// This overload may be called during standard activity processing, at which point the Bot's
        /// service URL and credentials that are part of the current activity processing pipeline
        /// will be used.
        /// </remarks>
        public async Task<ConversationsResult> GetConversationsAsync(ITurnContext context, string continuationToken, CancellationToken cancellationToken)
        {
            var connectorClient = context.Services.Get<IConnectorClient>();
            var results = await connectorClient.Conversations.GetConversationsAsync(continuationToken, cancellationToken).ConfigureAwait(false);
            return results;
        }

        /// <summary>Attempts to retrieve the token for a user that's in a login flow.
        /// </summary>
        /// <param name="context">Context for the current turn of conversation with the user.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="magicCode">(Optional) Optional user entered code to validate.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Token Response.</returns>
        public async Task<TokenResponse> GetUserTokenAsync(ITurnContext context, string connectionName, string magicCode, CancellationToken cancellationToken)
        {
            BotAssert.ContextNotNull(context);
            if (context.Activity.From == null || string.IsNullOrWhiteSpace(context.Activity.From.Id))
            {
                throw new ArgumentNullException("BotFrameworkAdapter.GetuserToken(): missing from or from.id");
            }

            if (string.IsNullOrWhiteSpace(connectionName))
            {
                throw new ArgumentNullException(nameof(connectionName));
            }

            var client = CreateOAuthApiClient(context);
            return await client.GetUserTokenAsync(context.Activity.From.Id, connectionName, magicCode, null, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the raw signin link to be sent to the user for signin for a connection name.
        /// </summary>
        /// <param name="context">Context for the current turn of conversation with the user.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task completes successfully, the result contains the raw signin link.</remarks>
        public async Task<string> GetOauthSignInLinkAsync(ITurnContext context, string connectionName, CancellationToken cancellationToken)
        {
            BotAssert.ContextNotNull(context);
            if (string.IsNullOrWhiteSpace(connectionName))
            {
                throw new ArgumentNullException(nameof(connectionName));
            }

            var client = CreateOAuthApiClient(context);
            return await client.GetSignInLinkAsync(context.Activity, connectionName, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Signs the user out with the token server.
        /// </summary>
        /// <param name="context">Context for the current turn of conversation with the user.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task SignOutUserAsync(ITurnContext context, string connectionName, CancellationToken cancellationToken)
        {
            BotAssert.ContextNotNull(context);
            if (string.IsNullOrWhiteSpace(connectionName))
            {
                throw new ArgumentNullException(nameof(connectionName));
            }

            var client = CreateOAuthApiClient(context);
            await client.SignOutUserAsync(context.Activity.From.Id, connectionName, cancellationToken).ConfigureAwait(false);
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
            eventActivity.Conversation = new ConversationAccount(id: result.Id);
            eventActivity.Recipient = conversationParameters.Bot;

            using (TurnContext context = new TurnContext(this, (Activity)eventActivity))
            {
                ClaimsIdentity claimsIdentity = new ClaimsIdentity();
                claimsIdentity.AddClaim(new Claim(AuthenticationConstants.AudienceClaim, credentials.MicrosoftAppId));
                claimsIdentity.AddClaim(new Claim(AuthenticationConstants.AppIdClaim, credentials.MicrosoftAppId));
                claimsIdentity.AddClaim(new Claim(AuthenticationConstants.ServiceUrlClaim, serviceUrl));

                context.Services.Add<IIdentity>(BotIdentityKey, claimsIdentity);
                context.Services.Add(connectorClient);
                await RunPipelineAsync(context, callback, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Checks the current operating environment to determine whether to emulate the client for OAuth requests.
        /// </summary>
        /// <param name="turnContext">The context object for the current turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task completes successfully, the result indicates whether the adapter will
        /// emaulate an OAuth client.</remarks>
        protected async Task<bool> TrySetEmulatingOAuthCardsAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            if (!_isEmulatingOAuthCards &&
                string.Equals(turnContext.Activity.ChannelId, "emulator", StringComparison.InvariantCultureIgnoreCase) &&
                (await _credentialProvider.IsAuthenticationDisabledAsync().ConfigureAwait(false)))
            {
                _isEmulatingOAuthCards = true;
            }

            return _isEmulatingOAuthCards;
        }

        /// <summary>
        /// Creates an OAuth client for the bot.
        /// </summary>
        /// <param name="context">The context object for the current turn.</param>
        /// <returns>An OAuth client for the bot.</returns>
        protected OAuthClient CreateOAuthApiClient(ITurnContext context)
        {
            var client = context.Services.Get<IConnectorClient>() as ConnectorClient;
            if (client == null)
            {
                throw new ArgumentNullException("CreateOAuthApiClient: OAuth requires a valid ConnectorClient instance");
            }

            if (_isEmulatingOAuthCards)
            {
                return new OAuthClient(client, context.Activity.ServiceUrl);
            }

            return new OAuthClient(client, AuthenticationConstants.OAuthUrl);
        }

        /// <summary>
        /// Creates the connector client asynchronous.
        /// </summary>
        /// <param name="serviceUrl">The service URL.</param>
        /// <param name="claimsIdentity">The claims identity.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>ConnectorClient instance.</returns>
        /// <exception cref="NotSupportedException">ClaimsIdemtity cannot be null. Pass Anonymous ClaimsIdentity if authentication is turned off.</exception>
        private async Task<IConnectorClient> CreateConnectorClientAsync(string serviceUrl, ClaimsIdentity claimsIdentity, CancellationToken cancellationToken)
        {
            if (claimsIdentity == null)
            {
                throw new NotSupportedException("ClaimsIdemtity cannot be null. Pass Anonymous ClaimsIdentity if authentication is turned off.");
            }

            // For requests from channel App Id is in Audience claim of JWT token. For emulator it is in AppId claim. For
            // unauthenticated requests we have anonymouse identity provided auth is disabled.
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
        private IConnectorClient CreateConnectorClient(string serviceUrl, MicrosoftAppCredentials appCredentials = null)
        {
            ConnectorClient connectorClient;
            if (appCredentials != null)
            {
                connectorClient = new ConnectorClient(new Uri(serviceUrl), appCredentials);
            }
            else
            {
                connectorClient = new ConnectorClient(new Uri(serviceUrl));
            }

            if (_connectorClientRetryPolicy != null)
            {
                connectorClient.SetRetryPolicy(_connectorClientRetryPolicy);
            }

            return connectorClient;
        }

        /// <summary>
        /// Gets the application credentials. App Credentials are cached so as to ensure we are not refreshing
        /// token everytime.
        /// </summary>
        /// <param name="appId">The application identifier (AAD Id for the bot).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>App credentials.</returns>
        private async Task<MicrosoftAppCredentials> GetAppCredentialsAsync(string appId, CancellationToken cancellationToken)
        {
            if (appId == null)
            {
                return MicrosoftAppCredentials.Empty;
            }

            if (!_appCredentialMap.TryGetValue(appId, out var appCredentials))
            {
                string appPassword = await _credentialProvider.GetAppPasswordAsync(appId).ConfigureAwait(false);
                appCredentials = new MicrosoftAppCredentials(appId, appPassword);
                _appCredentialMap[appId] = appCredentials;
            }

            return appCredentials;
        }
    }
}
