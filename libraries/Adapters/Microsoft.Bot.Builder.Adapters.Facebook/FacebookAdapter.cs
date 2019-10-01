// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Builder.Adapters.Facebook
{
    public class FacebookAdapter : BotAdapter, IBotFrameworkHttpAdapter
    {
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
        /// Get a Facebook API client with the correct credentials based on the page identified in the incoming activity.
        /// This is used by many internal functions to get access to the Facebook API, and is exposed as `bot.api` on any BotWorker instances passed into Botkit handler functions.
        /// </summary>
        /// <param name="activity">An incoming message activity.</param>
        /// <returns>A Facebook API client.</returns>
        public async Task<FacebookClientWrapper> GetAPIAsync(Activity activity)
        {
            if (!string.IsNullOrWhiteSpace(_facebookClient.Options.AccessToken))
            {
                return new FacebookClientWrapper(new FacebookAdapterOptions(_facebookClient.Options.VerifyToken, _facebookClient.Options.AppSecret, _facebookClient.Options.AccessToken));
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(activity.Recipient?.Id))
                {
                    var pageId = activity.Recipient.Id;

                    if ((activity.ChannelData as dynamic)?.message != null && (activity.ChannelData as dynamic)?.message.is_echo)
                    {
                        pageId = activity.From.Id;
                    }

                    string token = await _facebookClient.Options.GetAccessTokenForPageAsync(pageId).ConfigureAwait(false);

                    if (string.IsNullOrWhiteSpace(token))
                    {
                        // error: missing credentials
                    }

                    return new FacebookClientWrapper(new FacebookAdapterOptions(_facebookClient.Options.VerifyToken, _facebookClient.Options.AppSecret, token));
                }
                else
                {
                    throw new Exception($"Unable to create API based on activity:{activity}");
                }
            }
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
            for (var i = 0; i < activities.Length; i++)
            {
                var activity = activities[i];
                if (activity.Type == ActivityTypes.Message)
                {
                    var message = ActivityToFacebook(activity);

                    var api = await GetAPIAsync(context.Activity).ConfigureAwait(false);
                    var res = await api.CallAPIAsync("/me/messages", message, null, cancellationToken).ConfigureAwait(false);

                    if (res != null)
                    {
                        var response = new ResourceResponse()
                        {
                            // Id = res.Content (res as dynamic).message_id,
                        };

                        responses.Add(response);
                    }
                }
                else
                {
                    // log error: unknown message type
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
            var request = reference.GetContinuationActivity().ApplyConversationReference(reference, true);

            using (var context = new TurnContext(this, request))
            {
                await RunPipelineAsync(context, logic, default(CancellationToken)).ConfigureAwait(false);
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
            if (await VerifySignatureAsync(request, response, cancellationToken).ConfigureAwait(false))
            {
                var facebookEvent = request.Body;
                if ((facebookEvent as dynamic).entry)
                {
                    for (var i = 0; i < (facebookEvent as dynamic).entry.Lenght; i++)
                    {
                        FacebookMessage[] payload = null;
                        var entry = (facebookEvent as dynamic).entry;

                        // handle normal incoming stuff
                        if (entry.changes != null)
                        {
                            payload = entry.changes;
                        }
                        else
                        {
                            payload = entry.messaging;
                        }

                        for (var j = 0; j < payload.Length; j++)
                        {
                            await ProcessSingleMessageAsync(payload[j], bot.OnTurnAsync, cancellationToken).ConfigureAwait(false);
                        }

                        // handle standby messages (this bot is not the active receiver)
                        if (entry.standby)
                        {
                            payload = entry.standby;

                            for (var j = 0; j < payload.Length; j++)
                            {
                                var message = payload[j];

                                // indicate that this message was received in standby mode rather than normal mode.
                                (message as dynamic).standby = true;
                                await ProcessSingleMessageAsync(message, bot.OnTurnAsync, cancellationToken).ConfigureAwait(false);
                            }
                        }
                    }

                    // send code 200
                    response.StatusCode = 200;
                    response.ContentType = "text/plain";
                    await response.WriteAsync(string.Empty, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Converts an Activity object to a Facebook messenger outbound message ready for the API.
        /// </summary>
        /// <param name="activity">The activity to be converted to Facebook message.</param>
        /// <returns>The resulting message.</returns>
        private FacebookMessage ActivityToFacebook(Activity activity)
        {
            var facebookMessage = new FacebookMessage(activity.Conversation.Id, new Message(), "RESPONSE");

            facebookMessage.Message.Text = activity.Text;

            // map these fields to their appropriate place
            if (activity.ChannelData != null)
            {
                facebookMessage.MessagingType = (activity.ChannelData as dynamic).messaging_type != null ? (activity.ChannelData as dynamic).messaging_type : null;

                facebookMessage.Tag = (activity.ChannelData as dynamic).tag != null ? (activity.ChannelData as dynamic).tag : null;

                facebookMessage.Message.StickerId = (activity.ChannelData as dynamic).sticker_id != null ? (activity.ChannelData as dynamic).sticker_id : null;

                facebookMessage.Message.Attachment = (activity.ChannelData as dynamic).attachment != null ? (activity.ChannelData as dynamic).sticker_id : null;

                facebookMessage.PersonaId = (activity.ChannelData as dynamic).persona_id != null ? (activity.ChannelData as dynamic).persona_id : null;

                facebookMessage.NotificationType = (activity.ChannelData as dynamic).notification_type != null ? (activity.ChannelData as dynamic).notification_type : null;

                facebookMessage.SenderAction = (activity.ChannelData as dynamic).sender_action != null ? (activity.ChannelData as dynamic).sender_action : null;

                // make sure the quick reply has a type
                if ((activity.ChannelData as dynamic).quick_replies != null)
                {
                    facebookMessage.Message.QuickReplies = (activity.ChannelData as dynamic).quick_replies; // TODO: Add the content_type depending of what shape quick_replies has
                }
            }

            return facebookMessage;
        }

        /// <summary>
        /// Handles each individual message inside a webhook payload (webhook may deliver more than one message at a time).
        /// </summary>
        /// <param name="message">The message to be processed.</param>
        /// <param name="logic">The callback logic to call upon the message.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task ProcessSingleMessageAsync(FacebookMessage message, BotCallbackHandler logic, CancellationToken cancellationToken)
        {
            if (message.SenderId == null)
            {
                message.SenderId = (message as dynamic).optin?.user_ref;
            }

            var activity = new Activity()
            {
                ChannelId = "facebook",
                Timestamp = new DateTime(),
                Conversation = new ConversationAccount()
                {
                    Id = message.SenderId,
                },
                From = new ChannelAccount()
                {
                    Id = message.SenderId,
                    Name = message.SenderId,
                },
                Recipient = new ChannelAccount()
                {
                    Id = message.RecipientId,
                    Name = message.RecipientId,
                },
                ChannelData = message,
                Type = ActivityTypes.Event,
                Text = null,
            };

            if (message.Message != null)
            {
                activity.Type = ActivityTypes.Message;
                activity.Text = message.Message.Text;

                if ((activity.ChannelData as dynamic).message.is_echo)
                {
                    activity.Type = ActivityTypes.Event;
                }

                // copy fields like attachments, sticker, quick_reply, nlp, etc. // TODO Check
                activity.ChannelData = message.Message;
            }
            else if ((message as dynamic).postback != null)
            {
                activity.Type = ActivityTypes.Message;
                activity.Text = (message as dynamic).postback.payload;
            }

            using (var context = new TurnContext(this, activity))
            {
                await RunPipelineAsync(context, logic, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Verifies the SHA1 signature of the raw request payload before bodyParser parses it will abort parsing if signature is invalid, and pass a generic error to response.
        /// </summary>
        /// <param name="request">An Http request object.</param>
        /// <param name="response">An Http response object.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task<bool> VerifySignatureAsync(HttpRequest request, HttpResponse response, CancellationToken cancellationToken)
        {
            var expected = request.Headers["x-hub-signature"];

            string calculated = null;
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_facebookClient.Options.AppSecret)))
            {
                using (var bodyStream = new StreamReader(request.Body))
                {
                    calculated = $"sha1={hmac.ComputeHash(Encoding.UTF8.GetBytes(bodyStream.ReadToEnd()))}";
                }
            }

            if (expected != calculated)
            {
                response.StatusCode = 401;
                response.ContentType = "text/plain";
                await response.WriteAsync(string.Empty, cancellationToken).ConfigureAwait(false);
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
