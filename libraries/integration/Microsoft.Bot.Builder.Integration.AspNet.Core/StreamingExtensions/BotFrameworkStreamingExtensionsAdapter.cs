// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Protocol;
using Microsoft.Bot.Protocol.WebSockets;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Rest;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.StreamingExtensions
{
    public class BotFrameworkStreamingExtensionsAdapter : BotAdapter, IBotFrameworkStreamingExtensionsAdapter
    {
        private const string InvokeReponseKey = "BotFrameworkAdapter.InvokeResponse";
        private readonly ILogger _logger;
        private readonly WebSocketServer _server;

        public BotFrameworkStreamingExtensionsAdapter(
            WebSocketServer webSocketServer,
            IMiddleware middleware = null,
            ILogger logger = null)
        {
            _logger = logger ?? NullLogger.Instance;
            _server = webSocketServer;

            if (middleware != null)
            {
                Use(middleware);
            }
        }

        /// <summary>
        /// Registers any middleware the adapter should include in the pipeline.
        /// </summary>
        /// <param name="middleware">The middleware to add to the pipeline.</param>
        /// <returns>This instance of the adapter.</returns>
        public new BotFrameworkStreamingExtensionsAdapter Use(IMiddleware middleware)
        {
            MiddlewareSet.Use(middleware);
            return this;
        }

        /// <summary>
        /// Overload for processing activities when given an authheader.
        /// </summary>
        /// <param name="authHeader">The auth token provided by the request.</param>
        /// <param name="activity">The activity to process.</param>
        /// <param name="callback">The BotCallBackHandler to call on completion.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The response to the activity.</returns>
        public async Task<InvokeResponse> ProcessActivityAsync(string authHeader, Activity activity, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            BotAssert.ActivityNotNull(activity);

            return await ProcessActivityAsync(activity, callback, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Primary adapter method for processing activities sent from channel.
        /// </summary>
        /// <param name="activity">The activity to process.</param>
        /// <param name="callback">The BotCallBackHandler to call on completion.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The response to the activity.</returns>
        public async Task<InvokeResponse> ProcessActivityAsync(Activity activity, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            BotAssert.ActivityNotNull(activity);

            _logger.LogInformation($"Received an incoming activity.  ActivityId: {activity.Id}");

            using (var context = new TurnContext(this, activity))
            {
                await RunPipelineAsync(context, callback, cancellationToken).ConfigureAwait(false);

                // Handle Invoke scenarios, which deviate from the request/response model in that
                // the Bot will return a specific body and return code.
                if (activity.Type == ActivityTypes.Invoke)
                {
                    var activityInvokeResponse = context.TurnState.Get<Activity>(InvokeReponseKey);
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
                    var delayMs = (int)activity.Value;
                    await Task.Delay(delayMs, cancellationToken).ConfigureAwait(false);

                    // No need to create a response. One will be created below.
                }
                else if (activity.Type == ActivityTypesEx.InvokeResponse)
                {
                    turnContext.TurnState.Add(InvokeReponseKey, activity);

                    // No need to create a response. One will be created below.
                }
                else if (activity.Type == ActivityTypes.Trace && activity.ChannelId != "emulator")
                {
                    // if it is a Trace activity we only send to the channel if it's the emulator.
                }

                var requestPath = $"/v3/conversations/{activity.Conversation.Id}/activities/{activity.Id}";
                var request = Request.CreatePost(requestPath);
                request.SetBody(activity);
                response = await SendRequestAsync<ResourceResponse>(request).ConfigureAwait(false);

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

        private async Task<T> SendRequestAsync<T>(Request request)
        {
            try
            {
                var serverResponse = await _server.SendAsync(request).ConfigureAwait(false);

                if (serverResponse.StatusCode == (int)HttpStatusCode.OK)
                {
                    return serverResponse.ReadBodyAsJson<T>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return default(T);
        }

        public override Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken) => throw new NotImplementedException();

        public override Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken) => throw new NotImplementedException();

        public async Task<ConversationsResult> GetConversationsAsync(string continuationToken = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var route = "v3/conversations";
            var request = Request.CreateGet(route);

            return await SendRequestAsync<ConversationsResult>(request).ConfigureAwait(false);
        }

        public async Task<ConversationResourceResponse> PostConversationAsync(ConversationParameters parameters, CancellationToken cancellationToken = default(CancellationToken))
        {
            var route = "v3/conversations";
            var request = Request.CreatePost(route);
            request.SetBody(parameters);

            return await SendRequestAsync<ConversationResourceResponse>(request).ConfigureAwait(false);
        }

        public async Task<ResourceResponse> PostToConversationAsync(string conversationId, Activity activity, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            var route = string.Format("v3/conversations/{0}/activities", conversationId);
            var request = Request.CreatePost(route);
            request.SetBody(activity);

            return await SendRequestAsync<ResourceResponse>(request).ConfigureAwait(false);
        }

        public async Task<ResourceResponse> PostConversationHistoryAsync(string conversationId, Transcript transcript, CancellationToken cancellationToken = default(CancellationToken))
        {
            var route = string.Format("v3/conversations/{0}/activities/history", conversationId);
            var request = Request.CreatePost(route);
            request.SetBody(transcript);

            return await SendRequestAsync<ResourceResponse>(request).ConfigureAwait(false);
        }

        public async Task<ResourceResponse> UpdateActivityAsync(string conversationId, string activityId, Activity activity, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            var route = string.Format("v3/conversations/{0}/activities/{1}", conversationId, activity.Id);
            var request = Request.CreatePut(route);
            request.SetBody(activity);

            return await SendRequestAsync<ResourceResponse>(request).ConfigureAwait(false);
        }

        public async Task<ResourceResponse> PostToActivityAsync(string conversationId, string activityId, Activity activity, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            var route = string.Format("v3/conversations/{0}/activities/{1}", conversationId, activity.Id);
            var request = Request.CreatePost(route);
            request.SetBody(activity);

            return await SendRequestAsync<ResourceResponse>(request).ConfigureAwait(false);
        }

        public async Task<HttpOperationResponse> DeleteActivityAsync(string conversationId, string activityId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var route = string.Format("v3/conversations/{0}/activities/{1}", conversationId, activityId);
            var request = Request.CreateDelete(route);

            return await SendRequestAsync<HttpOperationResponse>(request).ConfigureAwait(false);
        }

        public async Task<IList<ChannelAccount>> GetConversationMembersAsync(string conversationId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var route = string.Format("v3/conversations/{0}/members", conversationId);
            var request = Request.CreateGet(route);

            return await SendRequestAsync<IList<ChannelAccount>>(request).ConfigureAwait(false);
        }

        public async Task<PagedMembersResult> GetConversationPagedMembersAsync(string conversationId, int? pageSize = null, string continuationToken = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var route = string.Format("v3/conversations/{0}/pagedmembers", conversationId);
            var request = Request.CreateGet(route);

            return await SendRequestAsync<PagedMembersResult>(request).ConfigureAwait(false);
        }

        public async Task<HttpOperationResponse> DeleteConversationMemberAsync(string conversationId, string memberId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var route = string.Format("v3/conversations/{0}/members/{1}", conversationId, memberId);
            var request = Request.CreateDelete(route);

            return await SendRequestAsync<HttpOperationResponse>(request).ConfigureAwait(false);
        }

        public async Task<IList<ChannelAccount>> GetActivityMembersAsync(string conversationId, string activityId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var route = string.Format("v3/conversations/{0}/activities/{1}/members", conversationId, activityId);
            var request = Request.CreateGet(route);

            return await SendRequestAsync<IList<ChannelAccount>>(request).ConfigureAwait(false);
        }
    }
}
