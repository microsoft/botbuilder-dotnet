// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// An adapter that implements the Bot Framework Protocol and can be hosted in different cloud environmens both public and private.
    /// </summary>
    public abstract class CloudAdapterBase : BotAdapter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CloudAdapterBase"/> class.
        /// </summary>
        /// <param name="botFrameworkAuthentication">The cloud environment used for validating and creating tokens.</param>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        protected CloudAdapterBase(
            BotFrameworkAuthentication botFrameworkAuthentication,
            ILogger logger = null)
        {
            BotFrameworkAuthentication = botFrameworkAuthentication ?? throw new ArgumentNullException(nameof(botFrameworkAuthentication));
            Logger = logger ?? NullLogger.Instance;
        }

        /// <summary>
        /// Gets the <see cref="BotFrameworkAuthentication" /> instance for this adapter.
        /// </summary>
        /// <value>
        /// The <see cref="BotFrameworkAuthentication" /> instance for this adapter.
        /// </value>
        protected BotFrameworkAuthentication BotFrameworkAuthentication { get; private set; }

        /// <summary>
        /// Gets a <see cref="ILogger" /> to use within this adapter and its subclasses.
        /// </summary>
        /// <value>
        /// The <see cref="ILogger" /> instance for this adapter.
        /// </value>
        protected ILogger Logger { get; private set; }

        /// <inheritdoc/>
        public override async Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)
        {
            _ = turnContext ?? throw new ArgumentNullException(nameof(turnContext));
            _ = activities ?? throw new ArgumentNullException(nameof(activities));

            if (activities.Length == 0)
            {
                throw new ArgumentException("Expecting one or more activities, but the array was empty.", nameof(activities));
            }

            Logger.LogInformation($"SendActivitiesAsync for {activities.Length} activities.");

            var responses = new ResourceResponse[activities.Length];

            for (var index = 0; index < activities.Length; index++)
            {
                var activity = activities[index];

                activity.Id = null;
                var response = default(ResourceResponse);

                Logger.LogInformation($"Sending activity.  ReplyToId: {activity.ReplyToId}");

                if (activity.Type == ActivityTypesEx.Delay)
                {
                    var delayMs = (int)activity.Value;
                    await Task.Delay(delayMs, cancellationToken).ConfigureAwait(false);
                }
                else if (activity.Type == ActivityTypesEx.InvokeResponse)
                {
                    turnContext.TurnState.Add(InvokeResponseKey, activity);
                }
                else if (activity.Type == ActivityTypes.Trace && activity.ChannelId != Channels.Emulator)
                {
                    // no-op
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

                if (response == null)
                {
                    response = new ResourceResponse(activity.Id ?? string.Empty);
                }

                responses[index] = response;
            }

            return responses;
        }

        /// <inheritdoc/>
        public override async Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            _ = turnContext ?? throw new ArgumentNullException(nameof(turnContext));
            _ = activity ?? throw new ArgumentNullException(nameof(activity));

            Logger.LogInformation($"UpdateActivityAsync ActivityId: {activity.Id}");

            var connectorClient = turnContext.TurnState.Get<IConnectorClient>();
            return await connectorClient.Conversations.UpdateActivityAsync(activity, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public override async Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken)
        {
            _ = turnContext ?? throw new ArgumentNullException(nameof(turnContext));
            _ = reference ?? throw new ArgumentNullException(nameof(reference));

            Logger.LogInformation($"DeleteActivityAsync Conversation Id: {reference.Conversation.Id}, ActivityId: {reference.ActivityId}");

            var connectorClient = turnContext.TurnState.Get<IConnectorClient>();
            await connectorClient.Conversations.DeleteActivityAsync(reference.Conversation.Id, reference.ActivityId, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public override Task ContinueConversationAsync(string botAppId, ConversationReference reference, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            _ = reference ?? throw new ArgumentNullException(nameof(reference));

            return ProcessProactiveAsync(CreateClaimsIdentity(botAppId), reference.GetContinuationActivity(), null, callback, cancellationToken);
        }

        /// <inheritdoc/>
        public override Task ContinueConversationAsync(ClaimsIdentity claimsIdentity, ConversationReference reference, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            _ = reference ?? throw new ArgumentNullException(nameof(reference));

            return ProcessProactiveAsync(claimsIdentity, reference.GetContinuationActivity(), null, callback, cancellationToken);
        }

        /// <inheritdoc/>
        public override Task ContinueConversationAsync(ClaimsIdentity claimsIdentity, ConversationReference reference, string audience, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            _ = claimsIdentity ?? throw new ArgumentNullException(nameof(claimsIdentity));
            _ = reference ?? throw new ArgumentNullException(nameof(reference));
            _ = callback ?? throw new ArgumentNullException(nameof(callback));

            return ProcessProactiveAsync(claimsIdentity, reference.GetContinuationActivity(), audience, callback, cancellationToken);
        }

        /// <inheritdoc/>
        public override Task ContinueConversationAsync(string botAppId, Activity continuationActivity, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            _ = callback ?? throw new ArgumentNullException(nameof(callback));
            ValidateContinuationActivity(continuationActivity);

            return ProcessProactiveAsync(CreateClaimsIdentity(botAppId), continuationActivity, null, callback, cancellationToken);
        }

        /// <inheritdoc/>
        public override Task ContinueConversationAsync(ClaimsIdentity claimsIdentity, Activity continuationActivity, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            _ = claimsIdentity ?? throw new ArgumentNullException(nameof(claimsIdentity));
            _ = callback ?? throw new ArgumentNullException(nameof(callback));
            ValidateContinuationActivity(continuationActivity);

            return ProcessProactiveAsync(claimsIdentity, continuationActivity, null, callback, cancellationToken);
        }

        /// <inheritdoc/>
        public override Task ContinueConversationAsync(ClaimsIdentity claimsIdentity, Activity continuationActivity, string audience, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            _ = claimsIdentity ?? throw new ArgumentNullException(nameof(claimsIdentity));
            _ = callback ?? throw new ArgumentNullException(nameof(callback));
            ValidateContinuationActivity(continuationActivity);

            return ProcessProactiveAsync(claimsIdentity, continuationActivity, audience, callback, cancellationToken);
        }

        /// <summary>
        /// The implementation for continue conversation.
        /// </summary>
        /// <param name="claimsIdentity">A <see cref="ClaimsIdentity"/> for the conversation.</param>
        /// <param name="continuationActivity">The continuation <see cref="Activity"/> used to create the <see cref="ITurnContext" />.</param>
        /// <param name="audience">The audience for the call.</param>
        /// <param name="callback">The method to call for the resulting bot turn.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected async Task ProcessProactiveAsync(ClaimsIdentity claimsIdentity, Activity continuationActivity, string audience, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"ProcessProactiveAsync for Conversation Id: {continuationActivity.Conversation.Id}");

            // Create the connector factory and  the inbound request, extracting parameters and then create a connector for outbound requests.
            var connectorFactory = BotFrameworkAuthentication.CreateConnectorFactory(claimsIdentity);

            // Create the connector client to use for outbound requests.
            using (var connectorClient = await connectorFactory.CreateAsync(continuationActivity.ServiceUrl, audience, cancellationToken).ConfigureAwait(false))

            // Create a UserTokenClient instance for the application to use. (For example, in the OAuthPrompt.) 
            using (var userTokenClient = await BotFrameworkAuthentication.CreateUserTokenClientAsync(claimsIdentity, cancellationToken).ConfigureAwait(false))

            // Create a turn context and run the pipeline.
            using (var context = CreateTurnContext(continuationActivity, claimsIdentity, audience, connectorClient, userTokenClient, callback, connectorFactory))
            {
                // Run the pipeline.
                await RunPipelineAsync(context, callback, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The implementation for processing an Activity sent to this bot.
        /// </summary>
        /// <param name="authHeader">The authorization header from the http request.</param>
        /// <param name="activity">The <see cref="Activity"/> to process.</param>
        /// <param name="callback">The method to call for the resulting bot turn.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the work queued to execute. Containing the InvokeResponse if there is one.</returns>
        protected async Task<InvokeResponse> ProcessActivityAsync(string authHeader, Activity activity, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"ProcessActivityAsync");

            // Authenticate the inbound request, extracting parameters and create a ConnectorFactory for creating a Connector for outbound requests.
            var authenticateRequestResult = await BotFrameworkAuthentication.AuthenticateRequestAsync(activity, authHeader, cancellationToken).ConfigureAwait(false);

            // Delegate the creation and execution of the turn, so the implementation can be shared with streaming requests
            return await ProcessActivityAsync(authenticateRequestResult, activity, callback, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// The implementation for processing an Activity sent to this bot.
        /// </summary>
        /// <param name="authenticateRequestResult">The authentication results for this turn.</param>
        /// <param name="activity">The <see cref="Activity"/> to process.</param>
        /// <param name="callback">The method to call for the resulting bot turn.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the work queued to execute. Containing the InvokeResponse if there is one.</returns>
        protected async Task<InvokeResponse> ProcessActivityAsync(AuthenticateRequestResult authenticateRequestResult, Activity activity, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            // Set the callerId on the activity.
            activity.CallerId = authenticateRequestResult.CallerId;

            // Create the connector client to use for outbound requests.
            using (var connectorClient = await authenticateRequestResult.ConnectorFactory.CreateAsync(activity.ServiceUrl, authenticateRequestResult.Audience, cancellationToken).ConfigureAwait(false))

            // Create a UserTokenClient instance for the application to use. (For example, it would be used in a sign-in prompt.) 
            using (var userTokenClient = await BotFrameworkAuthentication.CreateUserTokenClientAsync(authenticateRequestResult.ClaimsIdentity, cancellationToken).ConfigureAwait(false))

            // Create a turn context and run the pipeline.
            using (var context = CreateTurnContext(activity, authenticateRequestResult.ClaimsIdentity, authenticateRequestResult.Audience, connectorClient, userTokenClient, callback, authenticateRequestResult.ConnectorFactory))
            {
                // Run the pipeline.
                await RunPipelineAsync(context, callback, cancellationToken).ConfigureAwait(false);

                // If there are any results they will have been left on the TurnContext. 
                return ProcessTurnResults(context);
            }
        }

        /// <summary>
        /// This is a helper to create the ClaimsIdentity structure from an appId that will be added to the TurnContext.
        /// It is intended for use in proactive and named-pipe scenarios.
        /// </summary>
        /// <param name="botAppId">The bot's application id.</param>
        /// <returns>A <see cref="ClaimsIdentity"/> with the audience and appId claims set to the appId.</returns>
        protected ClaimsIdentity CreateClaimsIdentity(string botAppId)
        {
            if (botAppId == null)
            {
                botAppId = string.Empty;
            }

            // Hand craft Claims Identity.
            return new ClaimsIdentity(new List<Claim>
            {
                // Adding claims for both Emulator and Channel.
                new Claim(AuthenticationConstants.AudienceClaim, botAppId),
                new Claim(AuthenticationConstants.AppIdClaim, botAppId),
            });
        }

        private TurnContext CreateTurnContext(Activity activity, ClaimsIdentity claimsIdentity, string oauthScope, IConnectorClient connectorClient, UserTokenClient userTokenClient, BotCallbackHandler callback, ConnectorFactory connectorFactory)
        {
            var turnContext = new TurnContext(this, activity);
            turnContext.TurnState.Add<IIdentity>(BotIdentityKey, claimsIdentity);
            turnContext.TurnState.Add(connectorClient);
            turnContext.TurnState.Add(userTokenClient);
            turnContext.TurnState.Add(callback);
            turnContext.TurnState.Add(connectorFactory);
            turnContext.TurnState.Set(OAuthScopeKey, oauthScope); // in non-skills scenarios the oauth scope value here will be null, so use Set

            return turnContext;
        }

        private void ValidateContinuationActivity(Activity continuationActivity)
        {
            _ = continuationActivity ?? throw new ArgumentNullException(nameof(continuationActivity));
            _ = continuationActivity.Conversation ?? throw new ArgumentException("The continuation Activity should contain a Conversation value.");
            _ = continuationActivity.ServiceUrl ?? throw new ArgumentException("The continuation Activity should contain a ServiceUrl value.");
        }

        private InvokeResponse ProcessTurnResults(TurnContext turnContext)
        {
            // Handle ExpectedReplies scenarios where the all the activities have been buffered and sent back at once in an invoke response.
            if (turnContext.Activity.DeliveryMode == DeliveryModes.ExpectReplies)
            {
                return new InvokeResponse { Status = (int)HttpStatusCode.OK, Body = new ExpectedReplies(turnContext.BufferedReplyActivities) };
            }

            // Handle Invoke scenarios where the Bot will return a specific body and return code.
            if (turnContext.Activity.Type == ActivityTypes.Invoke)
            {
                var activityInvokeResponse = turnContext.TurnState.Get<Activity>(InvokeResponseKey);
                if (activityInvokeResponse == null)
                {
                    return new InvokeResponse { Status = (int)HttpStatusCode.NotImplemented };
                }

                return (InvokeResponse)activityInvokeResponse.Value;
            }

            // No body to return.
            return null;
        }
    }
}
