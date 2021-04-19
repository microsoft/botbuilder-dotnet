// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Authentication;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Thrzn41.WebexTeams.Version1;

namespace Microsoft.Bot.Builder.Adapters.Webex
{
    /// <summary>
    /// BotAdapter to allow for handling Webex Teams app payloads and responses via the Webex Teams API.
    /// </summary>
    public class WebexAdapter : BotAdapter, IBotFrameworkHttpAdapter
    {
        private const string WebexAccessTokenKey = "WebexAccessToken";
        private const string WebexPublicAddressKey = "WebexPublicAddress";
        private const string WebexSecretKey = "WebexSecret";
        private const string WebexWebhookNameKey = "WebexWebhookName";

        private readonly WebexClientWrapper _webexClient;
        private readonly ILogger _logger;
        private readonly WebexAdapterOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebexAdapter"/> class using configuration settings.
        /// </summary>
        /// <param name="configuration">An <see cref="IConfiguration"/> instance.</param>
        /// <remarks>
        /// The configuration keys are:
        /// WebexAccessToken: An access token for the bot.
        /// WebexPublicAddress: The root URL of the bot application.
        /// WebexSecret: The secret used to validate incoming webhooks.
        /// WebexWebhookName: A name for the webhook subscription.
        /// </remarks>
        /// <param name="options">An instance of <see cref="WebexAdapterOptions"/>.</param>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        public WebexAdapter(IConfiguration configuration, WebexAdapterOptions options = null, ILogger logger = null)
            : this(new WebexClientWrapper(new WebexClientWrapperOptions(configuration[WebexAccessTokenKey], new Uri(configuration[WebexPublicAddressKey]), configuration[WebexSecretKey], configuration[WebexWebhookNameKey])), options, logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebexAdapter"/> class.
        /// Creates a Webex adapter.
        /// </summary>
        /// <param name="webexClient">A Webex API interface.</param>
        /// <param name="options">An instance of <see cref="WebexAdapterOptions"/>.</param>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        public WebexAdapter(WebexClientWrapper webexClient, WebexAdapterOptions options, ILogger logger = null)
        {
            _webexClient = webexClient ?? throw new ArgumentNullException(nameof(webexClient));
            _options = options ?? new WebexAdapterOptions();
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
                if (activity.Type != ActivityTypes.Message)
                {
                    _logger.LogTrace($"Unsupported Activity Type: '{activity.Type}'. Only Activities of type 'Message' are supported.");
                }
                else
                {
                    // transform activity into the webex message format
                    string recipientId;
                    var target = MessageTarget.PersonId;

                    if (activity.Conversation?.Id != null)
                    {
                        recipientId = activity.Conversation.Id;
                        target = MessageTarget.SpaceId;
                    }
                    else if (activity.Conversation == null && activity.Recipient?.Id != null)
                    {
                        recipientId = activity.Recipient.Id;
                    }
                    else if (activity.GetChannelData<WebhookEventData>()?.MessageData.PersonEmail != null)
                    {
                        recipientId = activity.GetChannelData<WebhookEventData>()?.MessageData.PersonEmail;
                    }
                    else
                    {
                        throw new InvalidOperationException("No Person, Email or Room to send the message");
                    }

                    string responseId;

                    if (activity.Attachments != null && activity.Attachments.Count > 0)
                    {
                        if (activity.Attachments[0].ContentType == "application/vnd.microsoft.card.adaptive")
                        {
                            responseId = await _webexClient.CreateMessageWithAttachmentsAsync(recipientId, activity.Text, activity.Attachments, MessageTextType.Text, target, cancellationToken).ConfigureAwait(false);
                        }
                        else
                        {
                            var files = new List<Uri>();

                            foreach (var attachment in activity.Attachments)
                            {
                                var file = new Uri(attachment.ContentUrl);
                                files.Add(file);
                            }

                            responseId = await _webexClient.CreateMessageAsync(recipientId, activity.Text, files.Count > 0 ? files : null, MessageTextType.Text, target, cancellationToken).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        responseId = await _webexClient
                            .CreateMessageAsync(recipientId, activity.Text, target: target, cancellationToken: cancellationToken)
                            .ConfigureAwait(false);
                    }

                    responses.Add(new ResourceResponse(responseId));
                }
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
        /// </list></para>
        /// </remarks>
        /// <seealso cref="BotAdapter.RunPipelineAsync(ITurnContext, BotCallbackHandler, CancellationToken)"/>
        public override async Task ContinueConversationAsync(ClaimsIdentity claimsIdentity, ConversationReference reference, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            using (var context = new TurnContext(this, reference.GetContinuationActivity()))
            {
                context.TurnState.Add<IIdentity>(BotIdentityKey, claimsIdentity);
                context.TurnState.Add<BotCallbackHandler>(callback);
                await RunPipelineAsync(context, callback, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Accept an incoming webhook <see cref="HttpRequest"/> and convert it into a <see cref="TurnContext"/> which can be processed by the bot's logic.
        /// </summary>
        /// <param name="request">The incoming <see cref="HttpRequest"/>.</param>
        /// <param name="response">When this method completes, the <see cref="HttpResponse"/> to send.</param>
        /// <param name="bot">The bot that will handle the incoming activity.</param>
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
                json = await bodyStream.ReadToEndAsync().ConfigureAwait(false);
                payload = JsonConvert.DeserializeObject<WebhookEventData>(json);
            }

            if (_options.ValidateIncomingRequests && !_webexClient.ValidateSignature(request, json))
            {
                throw new AuthenticationException("Webhook received message with invalid signature. Potential malicious behavior!");
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

        /// <summary>
        /// Determines whether the provided <see cref="IConfiguration"/> has the settings needed to
        /// configure a <see cref="WebexAdapter"/>.
        /// </summary>
        /// <param name="configuration"><see cref="IConfiguration"/> to verify for settings.</param>
        /// <returns>A value indicating whether the configuration has the necessary settings required to create a <see cref="WebexAdapter"/>.</returns>
        internal static bool HasConfiguration(IConfiguration configuration)
        {
            // Do we have the config needed to create an adapter?
            return !string.IsNullOrEmpty(configuration.GetValue<string>(WebexAccessTokenKey))
                && !string.IsNullOrEmpty(configuration.GetValue<string>(WebexPublicAddressKey))
                && !string.IsNullOrEmpty(configuration.GetValue<string>(WebexSecretKey))
                && !string.IsNullOrEmpty(configuration.GetValue<string>(WebexWebhookNameKey));
        }
    }
}
