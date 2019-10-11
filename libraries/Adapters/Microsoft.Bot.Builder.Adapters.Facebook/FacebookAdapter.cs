// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Facebook
{
    public class FacebookAdapter : BotAdapter, IBotFrameworkHttpAdapter
    {
        private const string HubModeSubscribe = "subscribe";

        private readonly FacebookClientWrapper _facebookClient;

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
        public FacebookAdapter(IConfiguration configuration)
            : this(new FacebookClientWrapper(new FacebookAdapterOptions(configuration["VerifyToken"], configuration["AppSecret"], configuration["AccessToken"])))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FacebookAdapter"/> class.
        /// Creates a Webex adapter.
        /// </summary>
        /// <param name="facebookClient">A Webex API interface.</param>
        public FacebookAdapter(FacebookClientWrapper facebookClient)
        {
            _facebookClient = facebookClient ?? throw new ArgumentNullException(nameof(facebookClient));
        }

        /// <summary>
        /// Standard BotBuilder adapter method to send a message from the bot to the messaging API.
        /// </summary>
        /// <param name="context">A TurnContext representing the current incoming message and environment.</param>
        /// <param name="activities">An array of outgoing activities to be sent back to the messaging API.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext context, Activity[] activities, CancellationToken cancellationToken)
        {
            var responses = new List<ResourceResponse>();

            foreach (var activity in activities)
            {
                if (activity.Type != ActivityTypes.Message)
                {
                    throw new Exception("Unknown message type");
                }

                var message = FacebookHelper.ActivityToFacebook(activity);

                if (message.Message.Attachment != null)
                {
                    message.Message.Attachments = null;
                    message.Message.Text = null;
                }

                var res = await _facebookClient.SendMessageAsync("/me/messages", message, null, cancellationToken).ConfigureAwait(false);

                var response = new ResourceResponse()
                {
                    Id = res,
                };

                responses.Add(response);
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
            // Facebook adapter does not support updateActivity.
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
            // Facebook adapter does not support deleteActivity.
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
        /// <param name="request">A request object.</param>
        /// <param name="response">A response object.</param>
        /// <param name="bot">A bot logic function.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ProcessAsync(HttpRequest request, HttpResponse response, IBot bot, CancellationToken cancellationToken)
        {
            if (request.Query["hub.mode"] == HubModeSubscribe)
            {
                await _facebookClient.VerifyWebhookAsync(request, response, cancellationToken).ConfigureAwait(false);
                return;
            }

            string stringifyBody;

            using (var sr = new StreamReader(request.Body))
            {
                stringifyBody = sr.ReadToEnd();
            }

            if (!_facebookClient.VerifySignature(request, stringifyBody))
            {
                await FacebookHelper.WriteAsync(response, HttpStatusCode.Unauthorized, string.Empty, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
                throw new Exception("WARNING: Webhook received message with invalid signature. Potential malicious behavior!");
            }

            FacebookResponseEvent facebookEvent = null;
            try
            {
                facebookEvent = JsonConvert.DeserializeObject<FacebookResponseEvent>(stringifyBody);
            }
            catch (Exception exp)
            {
                throw new Exception(exp.Message);
            }

            foreach (var entry in facebookEvent.Entry)
            {
                var payload = new List<FacebookMessage>();

                // handle normal incoming stuff
                payload = entry.Changes ?? entry.Messaging;

                foreach (var message in payload)
                {
                    var activity = FacebookHelper.ProcessSingleMessage(message);

                    using (var context = new TurnContext(this, activity))
                    {
                        await RunPipelineAsync(context, bot.OnTurnAsync, cancellationToken).ConfigureAwait(false);
                    }
                }

                // handle standby messages (this bot is not the active receiver)
                if (entry.Standby != null)
                {
                    payload = entry.Standby;

                    foreach (var message in payload)
                    {
                        // indicate that this message was received in standby mode rather than normal mode.
                        message.Standby = true;
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
}
