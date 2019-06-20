using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Protocol.Transport;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Net.Http.Headers;
using Microsoft.Rest;
using Newtonsoft.Json;

namespace Microsoft.Bot.Protocol.StreamingExtensions
{
    public class BotFrameworkStreamingExtensionsAdapter : BotAdapter
    {
        private const string InvokeReponseKey = "BotFrameworkStreamingExtensionsAdapter.InvokeResponse";
        private readonly ILogger _logger;
        private readonly IStreamingTransportServer _server;

        public BotFrameworkStreamingExtensionsAdapter(
            IStreamingTransportServer streamingTransportServer,
            IList<IMiddleware> middlewares = null,
            ILogger logger = null)
        {
            _logger = logger ?? NullLogger.Instance;
            _server = streamingTransportServer;

            if (middlewares != null)
            {
                foreach (var item in middlewares)
                {
                    Use(item);
                }
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
        /// Overload for processing activities when given the activity a json string.
        /// </summary>
        /// <param name="body">The json string to deserialize into an activity.</param>
        /// <param name="streams">A set of streams associated with but not attached to the activity.</param>
        /// <param name="callback">The BotCallBackHandler to call on completion.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The response to the activity.</returns>
        public async Task<InvokeResponse> ProcessActivityAsync(string body, List<IContentStream> streams, BotCallbackHandler callback, CancellationToken cancellation)
        {
            var activity = JsonConvert.DeserializeObject<Activity>(body, SerializationSettings.DefaultDeserializationSettings);

            if (streams.Count > 1)
            {
                var streamAttachments = new List<Attachment>();
                for (int i = 1; i < streams.Count; i++)
                {
                    streamAttachments.Add(new Attachment() { ContentType = streams[i].Type, Content = streams[i].GetStream() });
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

            return await ProcessActivityAsync(activity, callback, cancellation);
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
                var activity = activities[index] ?? throw new ArgumentNullException("Found null activity in SendActivitiesAsync.");
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
                var request = Request.CreatePost(requestPath);
                request.SetBody(activity);
                if (streamAttachments != null)
                {
                    foreach (var attachment in streamAttachments)
                    {
                        request.AddStream(attachment);
                    }
                }
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

        public override async Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            var requestPath = $"/v3/conversations/{activity.Conversation.Id}/activities/{activity.Id}";
            var request = Request.CreatePut(requestPath);
            request.SetBody(activity);

            return await SendRequestAsync<ResourceResponse>(request, cancellationToken).ConfigureAwait(false);
        }

        public override async Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken)
        {
            if (reference == null)
            {
                throw new ArgumentNullException(nameof(reference));
            }

            var requestPath = $"/v3/conversations/{reference.Conversation.Id}/activities/{reference.ActivityId}";
            var request = Request.CreateDelete(requestPath);

            await SendRequestAsync<ResourceResponse>(request, cancellationToken).ConfigureAwait(false);
        }


        public async Task<ConversationsResult> GetConversationsAsync(string continuationToken = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var route = "/v3/conversations/";
            var request = Request.CreateGet(route);

            return await SendRequestAsync<ConversationsResult>(request, cancellationToken).ConfigureAwait(false);
        }

        public async Task<ConversationResourceResponse> PostConversationAsync(ConversationParameters parameters, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            var route = "/v3/conversations/";
            var request = Request.CreatePost(route);
            request.SetBody(parameters);

            return await SendRequestAsync<ConversationResourceResponse>(request, cancellationToken).ConfigureAwait(false);
        }

        public async Task<ResourceResponse> PostToConversationAsync(string conversationId, Activity activity, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            if (string.IsNullOrWhiteSpace(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            var route = string.Format("/v3/conversations/{0}/activities", conversationId);
            var request = Request.CreatePost(route);
            request.SetBody(activity);

            return await SendRequestAsync<ResourceResponse>(request, cancellationToken).ConfigureAwait(false);
        }

        public async Task<ResourceResponse> PostConversationHistoryAsync(string conversationId, Transcript transcript, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            var route = string.Format("/v3/conversations/{0}/activities/history", conversationId);
            var request = Request.CreatePost(route);
            request.SetBody(transcript);

            return await SendRequestAsync<ResourceResponse>(request, cancellationToken).ConfigureAwait(false);
        }

        public async Task<ResourceResponse> UpdateActivityAsync(Activity activity, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            var route = string.Format("/v3/conversations/{0}/activities/{1}", activity.Conversation.Id, activity.Id);
            var request = Request.CreatePut(route);
            request.SetBody(activity);

            return await SendRequestAsync<ResourceResponse>(request, cancellationToken).ConfigureAwait(false);
        }

        public async Task<ResourceResponse> PostToActivityAsync(Activity activity, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            var route = string.Format("/v3/conversations/{0}/activities/{1}", activity.Conversation.Id, activity.Id);
            var request = Request.CreatePost(route);
            request.SetBody(activity);

            return await SendRequestAsync<ResourceResponse>(request, cancellationToken).ConfigureAwait(false);
        }

        public async Task<HttpOperationResponse> DeleteActivityAsync(string conversationId, string activityId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            if (string.IsNullOrWhiteSpace(activityId))
            {
                throw new ArgumentNullException(nameof(activityId));
            }

            var route = string.Format("/v3/conversations/{0}/activities/{1}", conversationId, activityId);
            var request = Request.CreateDelete(route);

            return await SendRequestAsync<HttpOperationResponse>(request, cancellationToken).ConfigureAwait(false);
        }

        public async Task<IList<ChannelAccount>> GetConversationMembersAsync(string conversationId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            var route = string.Format("/v3/conversations/{0}/members", conversationId);
            var request = Request.CreateGet(route);

            return await SendRequestAsync<IList<ChannelAccount>>(request, cancellationToken).ConfigureAwait(false);
        }

        public async Task<PagedMembersResult> GetConversationPagedMembersAsync(string conversationId, int? pageSize = null, string continuationToken = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            var route = string.Format("/v3/conversations/{0}/pagedmembers", conversationId);
            var request = Request.CreateGet(route);

            return await SendRequestAsync<PagedMembersResult>(request, cancellationToken).ConfigureAwait(false);
        }

        public async Task<HttpOperationResponse> DeleteConversationMemberAsync(string conversationId, string memberId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            if (string.IsNullOrWhiteSpace(memberId))
            {
                throw new ArgumentNullException(nameof(memberId));
            }

            var route = string.Format("/v3/conversations/{0}/members/{1}", conversationId, memberId);
            var request = Request.CreateDelete(route);

            return await SendRequestAsync<HttpOperationResponse>(request, cancellationToken).ConfigureAwait(false);
        }

        public async Task<IList<ChannelAccount>> GetActivityMembersAsync(string conversationId, string activityId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            if (string.IsNullOrWhiteSpace(activityId))
            {
                throw new ArgumentNullException(nameof(activityId));
            }

            var route = string.Format("/v3/conversations/{0}/activities/{1}/members", conversationId, activityId);
            var request = Request.CreateGet(route);

            return await SendRequestAsync<IList<ChannelAccount>>(request, cancellationToken).ConfigureAwait(false);
        }

        private async Task<T> SendRequestAsync<T>(Request request, CancellationToken cancellation = default(CancellationToken))
        {
            try
            {
                var serverResponse = await _server.SendAsync(request, cancellation).ConfigureAwait(false);

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

        private IEnumerable<HttpContent> UpdateAttachmentStreams(Activity activity)
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
                    streamContent.Headers.TryAddWithoutValidation(HeaderNames.ContentType, streamAttachment.ContentType);
                    return streamContent;
                });
            }

            return null;
        }
    }
}
