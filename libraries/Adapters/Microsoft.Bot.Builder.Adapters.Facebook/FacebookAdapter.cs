// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents;
using Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents.Handover;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Facebook
{
    public class FacebookAdapter : BotAdapter, IBotFrameworkHttpAdapter
    {
        private const string HubModeSubscribe = "subscribe";

        /// <summary>
        /// An instance of the FacebookClientWrapper class.
        /// </summary>
        private readonly FacebookClientWrapper _facebookClient;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FacebookAdapter"/> class using configuration settings.
        /// </summary>
        /// <param name="configuration">An <see cref="IConfiguration"/> instance.</param>
        /// <remarks>
        /// The configuration keys are:
        /// VerifyToken: The token to respond to the initial verification request.
        /// AppSecret: The secret used to validate incoming webhooks.
        /// AccessToken: An access token for the bot.
        /// </remarks>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        public FacebookAdapter(IConfiguration configuration, ILogger logger = null)
            : this(new FacebookClientWrapper(new FacebookAdapterOptions(configuration["FacebookVerifyToken"], configuration["FacebookAppSecret"], configuration["FacebookAccessToken"])), logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FacebookAdapter"/> class.
        /// Creates a Facebook adapter.
        /// </summary>
        /// <param name="facebookClient">A Facebook API interface.</param>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        public FacebookAdapter(FacebookClientWrapper facebookClient, ILogger logger = null)
        {
            _facebookClient = facebookClient ?? throw new ArgumentNullException(nameof(facebookClient));
            _logger = logger ?? NullLogger.Instance;
        }

        /// <summary>
        /// Standard BotBuilder adapter method to send a message from the bot to the messaging API.
        /// </summary>
        /// <param name="turnContext">A TurnContext representing the current incoming message and environment.</param>
        /// <param name="activities">An array of outgoing activities to be sent back to the messaging API.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)
        {
            var responses = new List<ResourceResponse>();

            foreach (var activity in activities)
            {
                if (activity.Type != ActivityTypes.Message && activity.Type != ActivityTypes.Event)
                {
                    _logger.LogTrace($"Unsupported Activity Type: '{activity.Type}'. Only Activities of type 'Message' or 'Event' are supported.");
                }
                else
                {
                    var message = FacebookHelper.ActivityToFacebook(activity);

                    if (message.Message.Attachment != null)
                    {
                        message.Message.Text = null;
                    }

                    var res = await _facebookClient.SendMessageAsync("/me/messages", message, null, cancellationToken)
                        .ConfigureAwait(false);

                    if (activity.Type == ActivityTypes.Event)
                    {
                        if (activity.Name.Equals(HandoverConstants.PassThreadControl, StringComparison.Ordinal))
                        {
                            var recipient = (string)activity.Value == "inbox" ? HandoverConstants.PageInboxId : (string)activity.Value;
                            await _facebookClient.PassThreadControlAsync(recipient, activity.Conversation.Id, HandoverConstants.MetadataPassThreadControl).ConfigureAwait(false);
                        }
                        else if (activity.Name.Equals(HandoverConstants.TakeThreadControl, StringComparison.Ordinal))
                        {
                            await _facebookClient.TakeThreadControlAsync(activity.Conversation.Id, HandoverConstants.MetadataTakeThreadControl).ConfigureAwait(false);
                        }
                        else if (activity.Name.Equals(HandoverConstants.RequestThreadControl, StringComparison.Ordinal))
                        {
                            await _facebookClient.RequestThreadControlAsync(activity.Conversation.Id, HandoverConstants.MetadataRequestThreadControl).ConfigureAwait(false);
                        }
                    }

                    var response = new ResourceResponse()
                    {
                        Id = res,
                    };

                    responses.Add(response);
                }
            }

            return responses.ToArray();
        }

        /// <summary>
        /// Standard BotBuilder adapter method to update a previous message with new content.
        /// </summary>
        /// <param name="turnContext">A TurnContext representing the current incoming message and environment.</param>
        /// <param name="activity">The updated activity in the form '{id: `id of activity to update`, ...}'.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A resource response with the Id of the updated activity.</returns>
        public override Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            return Task.FromException<ResourceResponse>(new NotImplementedException("Facebook adapter does not support updateActivity."));
        }

        /// <summary>
        /// Standard BotBuilder adapter method to delete a previous message.
        /// </summary>
        /// <param name="turnContext">A TurnContext representing the current incoming message and environment.</param>
        /// <param name="reference">An object in the form "{activityId: `id of message to delete`, conversation: { id: `id of channel`}}".</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken)
        {
            return Task.FromException(new NotImplementedException("Facebook adapter does not support deleteActivity."));
        }

        /// <summary>
        /// Standard BotBuilder adapter method for continuing an existing conversation based on a conversation reference.
        /// </summary>
        /// <param name="reference">A conversation reference to be applied to future messages.</param>
        /// <param name="logic">A bot logic function that will perform continuing action in the form `async(context) => { ... }`.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ContinueConversationAsync(ConversationReference reference, BotCallbackHandler logic)
        {
            if (reference == null)
            {
                throw new ArgumentNullException(nameof(reference));
            }

            if (logic == null)
            {
                throw new ArgumentNullException(nameof(logic));
            }

            var request = reference.GetContinuationActivity().ApplyConversationReference(reference, true);

            using (var context = new TurnContext(this, request))
            {
                await RunPipelineAsync(context, logic, default).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Accept an incoming webhook request and convert it into a TurnContext which can be processed by the bot's logic.
        /// </summary>
        /// <param name="httpRequest">A request object.</param>
        /// <param name="httpResponse">A response object.</param>
        /// <param name="bot">A bot logic function.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ProcessAsync(HttpRequest httpRequest, HttpResponse httpResponse, IBot bot, CancellationToken cancellationToken = default)
        {
            if (httpRequest.Query["hub.mode"] == HubModeSubscribe)
            {
                await _facebookClient.VerifyWebhookAsync(httpRequest, httpResponse, cancellationToken).ConfigureAwait(false);
                return;
            }

            string stringifiedBody;

            using (var sr = new StreamReader(httpRequest.Body))
            {
                stringifiedBody = sr.ReadToEnd();
            }

            if (!_facebookClient.VerifySignature(httpRequest, stringifiedBody))
            {
                await FacebookHelper.WriteAsync(httpResponse, HttpStatusCode.Unauthorized, string.Empty, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
                throw new AuthenticationException("WARNING: Webhook received message with invalid signature. Potential malicious behavior!");
            }

            FacebookResponseEvent facebookResponseEvent = null;

            facebookResponseEvent = JsonConvert.DeserializeObject<FacebookResponseEvent>(stringifiedBody);

            foreach (var entry in facebookResponseEvent.Entry)
            {
                var payload = entry.Changes.Count > 0 ? entry.Changes : entry.Messaging.Count > 0 ? entry.Messaging : entry.Standby.Count > 0 ? entry.Standby : new List<FacebookMessage>();

                foreach (var message in payload)
                {
                    message.IsStandby = entry.Standby.Count > 0;

                    var activity = FacebookHelper.ProcessSingleMessage(message);

                    using (var context = new TurnContext(this, activity))
                    {
                        await RunPipelineAsync(context, bot.OnTurnAsync, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
        }
    }
}
