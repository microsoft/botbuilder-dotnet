// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Thrzn41.WebexTeams.Version1;

namespace Microsoft.Bot.Builder.Adapters.Webex
{
    public class WebexAdapter : BotAdapter, IBotFrameworkHttpAdapter
    {
        private readonly WebexClientWrapper _webexClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebexAdapter"/> class using configuration settings.
        /// </summary>
        /// <param name="configuration">An <see cref="IConfiguration"/> instance.</param>
        /// <remarks>
        /// The configuration keys are:
        /// AccessToken: An access token for the bot.
        /// PublicAddress: The root URL of the bot application.
        /// Secret: The secret used to validate incoming webhooks.
        /// WebhookName: A name for the webhook subscription.
        /// </remarks>
        public WebexAdapter(IConfiguration configuration)
            : this(new WebexClientWrapper(new WebexAdapterOptions(configuration["AccessToken"], new Uri(configuration["PublicAddress"]), configuration["Secret"], configuration["WebhookName"])))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebexAdapter"/> class.
        /// Creates a Webex adapter.
        /// </summary>
        /// <param name="webexClient">A Webex API interface.</param>
        public WebexAdapter(WebexClientWrapper webexClient)
        {
            _webexClient = webexClient ?? throw new ArgumentNullException(nameof(webexClient));
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
                if (activity.Type != ActivityTypes.Message)
                {
                    throw new Exception("Unknown message type");
                }

                // transform activity into the webex message format
                string personIdOrEmail;

                if (activity.GetChannelData<WebhookEventData>()?.MessageData.PersonEmail != null)
                {
                    personIdOrEmail = activity.GetChannelData<WebhookEventData>()?.MessageData.PersonEmail;
                }
                else
                {
                    if (activity.Recipient?.Id != null)
                    {
                        personIdOrEmail = activity.Recipient.Id;
                    }
                    else
                    {
                        throw new Exception("No Person or Email to send the message");
                    }
                }

                string responseId;

                if (activity.Attachments != null && activity.Attachments.Count > 0)
                {
                    if (activity.Attachments[0].ContentType == "application/vnd.microsoft.card.adaptive")
                    {
                        responseId = await _webexClient.CreateMessageWithAttachmentsAsync(personIdOrEmail, activity.Text, activity.Attachments, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        var files = new List<Uri>();

                        foreach (var attachment in activity.Attachments)
                        {
                            var file = new Uri(attachment.ContentUrl);
                            files.Add(file);
                        }

                        responseId = await _webexClient.CreateMessageAsync(personIdOrEmail, activity.Text, files.Count > 0 ? files : null, cancellationToken).ConfigureAwait(false);
                    }
                }
                else
                {
                    responseId = await _webexClient.CreateMessageAsync(personIdOrEmail, activity.Text, cancellationToken: cancellationToken).ConfigureAwait(false);
                }

                responses.Add(new ResourceResponse(responseId));
            }

            return responses.ToArray();
        }

        /// <summary>
        /// Standard BotBuilder adapter method to update a previous message.
        /// </summary>
        /// <param name="turnContext">A TurnContext representing the current incoming message and environment.</param>
        /// <param name="activity">An activity to be sent back to the messaging API.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            // Webex adapter does not support updateActivity.
            return Task.FromException<ResourceResponse>(new NotSupportedException("Webex adapter does not support updateActivity."));
        }

        /// <summary>
        /// Standard BotBuilder adapter method to delete a previous message.
        /// </summary>
        /// <param name="turnContext">A <see cref="ITurnContext"/> representing the current incoming message and environment.</param>
        /// <param name="reference">A <see cref="ConversationReference"/> object.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(reference.ActivityId))
            {
                await _webexClient.DeleteMessageAsync(reference.ActivityId, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Standard BotBuilder adapter method for continuing an existing conversation based on a conversation reference.
        /// </summary>
        /// <param name="reference">A <see cref="ConversationReference"/> to be applied to future messages.</param>
        /// <param name="logic">A bot logic function that will perform continuing action.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ContinueConversationAsync(ConversationReference reference, BotCallbackHandler logic, CancellationToken cancellationToken)
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
                await RunPipelineAsync(context, logic, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Accept an incoming webhook <see cref="HttpRequest"/> and convert it into a <see cref="TurnContext"/> which can be processed by the bot's logic.
        /// </summary>
        /// <param name="request">A <see cref="HttpRequest"/> object.</param>
        /// <param name="response">A <see cref="HttpResponse"/> object.</param>
        /// <param name="bot">A bot with logic function in the form.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ProcessAsync(HttpRequest request, HttpResponse response, IBot bot, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            if (bot == null)
            {
                throw new ArgumentNullException(nameof(bot));
            }

            var identity = await _webexClient.GetMeAsync(cancellationToken).ConfigureAwait(false);

            WebhookEventData payload;
            string json;
            using (var bodyStream = new StreamReader(request.Body))
            {
                json = bodyStream.ReadToEnd();
                payload = JsonConvert.DeserializeObject<WebhookEventData>(json);
            }

            if (!_webexClient.ValidateSignature(request, json))
            {
                throw new Exception("WARNING: Webhook received message with invalid signature. Potential malicious behavior!");
            }

            Activity activity;
            if (payload.Resource == EventResource.Message && payload.EventType == EventType.Created)
            {
                var decryptedMessage = await WebexHelper.GetDecryptedMessageAsync(payload, _webexClient.GetMessageAsync, cancellationToken).ConfigureAwait(false);

                activity = WebexHelper.DecryptedMessageToActivity(decryptedMessage, identity);
            }
            else if (payload.Resource.Name == "attachmentActions" && payload.EventType == EventType.Created)
            {
                var extraData = payload.GetResourceData<TeamsData>();

                var data = JsonConvert.SerializeObject(extraData);

                var jsonData = JsonConvert.DeserializeObject<AttachmentActionData>(data);

                var decryptedMessage = await _webexClient.GetAttachmentActionAsync(jsonData.Id, cancellationToken).ConfigureAwait(false);

                activity = WebexHelper.AttachmentActionToActivity(decryptedMessage, identity);
            }
            else
            {
                activity = WebexHelper.PayloadToActivity(payload, identity);
            }

            using (var context = new TurnContext(this, activity))
            {
                await RunPipelineAsync(context, bot.OnTurnAsync, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
