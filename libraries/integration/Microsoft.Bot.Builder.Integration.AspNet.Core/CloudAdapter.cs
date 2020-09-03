// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Rest;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    /// <summary>
    /// An adapter that implements the Bot Framework Protocol and can be hosted in different cloud environmens both public and private.
    /// </summary>
    public class CloudAdapter : BotAdapter, IBotFrameworkHttpAdapter
    {
        internal const string InvokeResponseKey = "BotFrameworkAdapter.InvokeResponse";

        private ICredentialProvider _credentialProvider;
        private ICloudEnvironmentProvider _cloudEnvironmentProvider;
        private IHttpClientFactory _httpClientFactory;
        private ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudAdapter"/> class.
        /// </summary>
        /// <param name="credentialProvider">The credential provider.</param>
        /// <param name="cloudEnvironmentProvider">The authentication configuration.</param>
        /// <param name="httpClientFactory">The IHttpClientFactory implementation this adapter should use.</param>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        public CloudAdapter(
            ICredentialProvider credentialProvider,
            ICloudEnvironmentProvider cloudEnvironmentProvider,
            ILogger logger = null,
            IHttpClientFactory httpClientFactory = null)
        {
            _credentialProvider = credentialProvider;
            _cloudEnvironmentProvider = cloudEnvironmentProvider;
            _httpClientFactory = httpClientFactory;
            _logger = logger ?? NullLogger.Instance;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudAdapter"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration instance.</param>
        /// <param name="httpClientFactory">The IHttpClientFactory implementation this adapter should use.</param>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        public CloudAdapter(
            IConfiguration configuration,
            ILogger logger = null,
            IHttpClientFactory httpClientFactory = null)
            : this(
                  new ConfigurationCredentialProvider(configuration),
                  new ConfigurationCloudEnvironmentProvider(configuration),
                  logger,
                  httpClientFactory)
        {
        }

        /// <inheritdoc/>
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

            if (bot == null)
            {
                throw new ArgumentNullException(nameof(bot));
            }

            if (httpRequest.Method == HttpMethods.Get)
            {
                throw new NotImplementedException("web sockets is not yet implemented");
            }
            else
            {
                // Deserialize the incoming Activity
                var activity = await HttpHelper.ReadRequestAsync<Activity>(httpRequest).ConfigureAwait(false);

                if (string.IsNullOrEmpty(activity?.Type))
                {
                    httpResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                    return;
                }

                // Grab the auth header from the inbound http request
                var authHeader = httpRequest.Headers["Authorization"];

                try
                {
                    // Process the inbound activity with the bot
                    var invokeResponse = await ProcessActivityAsync(authHeader, activity, bot.OnTurnAsync, cancellationToken).ConfigureAwait(false);

                    // write the response, potentially serializing the InvokeResponse
                    await HttpHelper.WriteResponseAsync(httpResponse, invokeResponse).ConfigureAwait(false);
                }
                catch (UnauthorizedAccessException)
                {
                    // handle unauthorized here as this layer creates the http response
                    httpResponse.StatusCode = (int)HttpStatusCode.Unauthorized;
                }
            }
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
                else if (activity.Type == ActivityTypes.Trace && activity.ChannelId != "emulator")
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

        // TODO: continue conversation implementation

        // TODO: oauth prompt support

        /// <summary>
        /// Override this to specialize the creation of a connectror client.
        /// </summary>
        /// <param name="serviceUrl">The serviceUrl for this connectror client.</param>
        /// <param name="credentials">The credentials for this connectror client.</param>
        /// <returns>A new connector client.</returns>
        protected virtual IConnectorClient CreateConnectorClient(string serviceUrl, ServiceClientCredentials credentials)
        {
            // Create the http client for the connector to use. 
            var httpClient = _httpClientFactory?.CreateClient("connectorClient");

            // Create a new connector.
            return new ConnectorClient(new Uri(serviceUrl), credentials, httpClient);
        }

        private async Task<InvokeResponse> ProcessActivityAsync(string authHeader, Activity activity, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            // Create a cloud environment.
            var cloudEnvironment = await _cloudEnvironmentProvider.GetCloudEnvironmentAsync().ConfigureAwait(false);

            // Create the http client for the cloud environment to use
            var httpClient = _httpClientFactory?.CreateClient("cloudEnvironment");

            // Use the cloud environment to authenticate the inbound request and create credentials for outbound requests.
            var (claimsIdentity, credentials, scope, callerId) = await cloudEnvironment.AuthenticateRequestAsync(activity, authHeader, _credentialProvider, httpClient, _logger).ConfigureAwait(false);

            // Set the callerId on the activity.
            activity.CallerId = callerId;

            // create the connector client to use for outbound requests.
            var connectorClient = CreateConnectorClient(activity.ServiceUrl, credentials);

            // Create a turn context and run the pipeline.
            using (var context = CreateTurnContext(activity, claimsIdentity, scope, connectorClient, callback))
            {
                // Run the pipeline.
                await RunPipelineAsync(context, callback, cancellationToken).ConfigureAwait(false);

                // If there are any results they will have been left on the TurnContext. 
                return ProcessTurnResults(context);
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
