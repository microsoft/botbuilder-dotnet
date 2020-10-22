// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
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
        internal const string InvokeResponseKey = "BotFrameworkAdapter.InvokeResponse";

        private readonly BotFrameworkAuthentication _botFrameworkAuthentication;
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudAdapterBase"/> class.
        /// </summary>
        /// <param name="botFrameworkAuthentication">The cloud environment used for validating and creating tokens.</param>
        /// <param name="httpClient">The IHttpClientFactory implementation this adapter should use.</param>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        protected CloudAdapterBase(
            BotFrameworkAuthentication botFrameworkAuthentication,
            HttpClient httpClient = null,
            ILogger logger = null)
        {
            _botFrameworkAuthentication = botFrameworkAuthentication ?? throw new ArgumentNullException(nameof(botFrameworkAuthentication));
            _httpClient = httpClient;
            _logger = logger ?? NullLogger.Instance;
        }

        /// <inheritdoc/>
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

            for (var index = 0; index < activities.Length; index++)
            {
                var activity = activities[index];

                activity.Id = null;
                var response = default(ResourceResponse);

                _logger.LogInformation($"Sending activity.  ReplyToId: {activity.ReplyToId}");

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
                    // TODO: implement CanProcessOutgoingActivity subclass contract

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
            var connectorClient = turnContext.TurnState.Get<IConnectorClient>();
            return await connectorClient.Conversations.UpdateActivityAsync(activity, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public override async Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken)
        {
            var connectorClient = turnContext.TurnState.Get<IConnectorClient>();
            await connectorClient.Conversations.DeleteActivityAsync(reference.Conversation.Id, reference.ActivityId, cancellationToken).ConfigureAwait(false);
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
        public override Task ContinueConversationAsync(string botAppId, ConversationReference reference, BotCallbackHandler callback, CancellationToken cancellationToken)
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

            // Hand craft Claims Identity.
            var claimsIdentity = new ClaimsIdentity(new List<Claim>
            {
                // Adding claims for both Emulator and Channel.
                new Claim(AuthenticationConstants.AudienceClaim, botAppId),
                new Claim(AuthenticationConstants.AppIdClaim, botAppId),
            });

            return ProcessProactiveAsync(claimsIdentity, reference, null, callback, cancellationToken);
        }

        /// <summary>
        /// Sends a proactive message from the bot to a conversation.
        /// </summary>
        /// <param name="claimsIdentity">A <see cref="ClaimsIdentity"/> for the conversation.</param>
        /// <param name="reference">A reference to the conversation to continue.</param>
        /// <param name="callback">The method to call for the resulting bot turn.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public override Task ContinueConversationAsync(ClaimsIdentity claimsIdentity, ConversationReference reference, BotCallbackHandler callback, CancellationToken cancellationToken)
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

            return ProcessProactiveAsync(claimsIdentity, reference, null, callback, cancellationToken);
        }

        /// <summary>
        /// Sends a proactive message from the bot to a conversation.
        /// </summary>
        /// <param name="claimsIdentity">A <see cref="ClaimsIdentity"/> for the conversation.</param>
        /// <param name="reference">A reference to the conversation to continue.</param>
        /// <param name="audience">The target audience for the connector.</param>
        /// <param name="callback">The method to call for the resulting bot turn.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public override Task ContinueConversationAsync(ClaimsIdentity claimsIdentity, ConversationReference reference, string audience, BotCallbackHandler callback, CancellationToken cancellationToken)
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

            return ProcessProactiveAsync(claimsIdentity, reference, audience, callback, cancellationToken);
        }

        // TODO: oauth prompt support

        /// <summary>
        /// The implementation for continue conversation.
        /// </summary>
        /// <param name="claimsIdentity">A <see cref="ClaimsIdentity"/> for the conversation.</param>
        /// <param name="reference">A <see cref="ConversationReference"/> for the conversation.</param>
        /// <param name="audience">The audience for the call.</param>
        /// <param name="callback">The method to call for the resulting bot turn.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected async Task ProcessProactiveAsync(ClaimsIdentity claimsIdentity, ConversationReference reference, string audience, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            // Use the cloud environment to create the credentials for proactive requests.
            var credentials = await _botFrameworkAuthentication.GetProactiveCredentialsAsync(claimsIdentity, audience, cancellationToken).ConfigureAwait(false);

            // Create the connector client to use for outbound requests.
            using (var connectorClient = new ConnectorClient(new Uri(reference.ServiceUrl), credentials, _httpClient, disposeHttpClient: _httpClient == null))
            {
                // Create a turn context and run the pipeline.
                using (var context = CreateTurnContext(reference.GetContinuationActivity(), claimsIdentity, audience, connectorClient, callback))
                {
                    // Run the pipeline.
                    await RunPipelineAsync(context, callback, cancellationToken).ConfigureAwait(false);

                    // Cleanup disposable resources in case other code kept a reference to it.
                    context.TurnState.Set<IConnectorClient>(null);
                }
            }
        }

        /// <summary>
        /// The implementation for processing an Activity sent to this bot.
        /// </summary>
        /// <param name="authHeader">The authorization header from teh http request.</param>
        /// <param name="activity">The <see cref="Activity"/> to process.</param>
        /// <param name="callback">The method to call for the resulting bot turn.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the work queued to execute. Containing the InvokeResponse if there is one.</returns>
        protected async Task<InvokeResponse> ProcessActivityAsync(string authHeader, Activity activity, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            // Use the cloud environment to authenticate the inbound request and create credentials for outbound requests.
            var authenticateRequestResult = await _botFrameworkAuthentication.AuthenticateRequestAsync(activity, authHeader, cancellationToken).ConfigureAwait(false);

            // Set the callerId on the activity.
            activity.CallerId = authenticateRequestResult.CallerId;

            // Create the connector client to use for outbound requests.
            using (var connectorClient = new ConnectorClient(new Uri(activity.ServiceUrl), authenticateRequestResult.Credentials, _httpClient, disposeHttpClient: _httpClient == null))
            {
                // Create a turn context and run the pipeline.
                using (var context = CreateTurnContext(activity, authenticateRequestResult.ClaimsIdentity, authenticateRequestResult.Scope, connectorClient, callback))
                {
                    // Run the pipeline.
                    await RunPipelineAsync(context, callback, cancellationToken).ConfigureAwait(false);

                    // Cleanup disposable resources in case other code kept a reference to it.
                    context.TurnState.Set<IConnectorClient>(null);

                    // If there are any results they will have been left on the TurnContext. 
                    return ProcessTurnResults(context);
                }
            }
        }

        private TurnContext CreateTurnContext(Activity activity, ClaimsIdentity claimsIdentity, string oauthScope, IConnectorClient connectorClient, BotCallbackHandler callback)
        {
            var turnContext = new TurnContext(this, activity);
            turnContext.TurnState.Add<IIdentity>(BotIdentityKey, claimsIdentity);
            turnContext.TurnState.Add(OAuthScopeKey, oauthScope);
            turnContext.TurnState.Add(connectorClient);
            turnContext.TurnState.Add(callback);
            return turnContext;
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
