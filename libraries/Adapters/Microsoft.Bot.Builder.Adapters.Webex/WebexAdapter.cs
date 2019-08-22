// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Thrzn41.WebexTeams;
using Thrzn41.WebexTeams.Version1;

namespace Microsoft.Bot.Builder.Adapters.Webex
{
    public class WebexAdapter : BotAdapter
    {
        private readonly IWebexAdapterOptions _config;

        private readonly IWebexClient _webexApi;

        private readonly TeamsAPIClient _api;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebexAdapter"/> class.
        /// Creates a Webex adapter. See <see cref="IWebexAdapterOptions"/> for a full definition of the allowed parameters.
        /// </summary>
        /// <param name="config">An object containing API credentials, a webhook verification token and other options.</param>
        /// <param name="webexApi">A Webex API interface.</param>
        public WebexAdapter(IWebexAdapterOptions config, IWebexClient webexApi)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            if (string.IsNullOrWhiteSpace(_config.AccessToken))
            {
                throw new Exception("AccessToken required to create controller");
            }

            if (string.IsNullOrWhiteSpace(_config.PublicAddress))
            {
                throw new Exception("PublicAddress parameter required to receive webhooks");
            }

            _config.PublicAddress = new Uri(_config.PublicAddress).Host;

            _webexApi = webexApi ?? throw new Exception("Could not create the Webex Teams API client");

            _webexApi.CreateClient(_config.AccessToken);
        }

        /// <summary>
        /// Gets or sets the identity of the bot.
        /// </summary>
        private Person Identity { get; set; }

        /// <summary>
        /// Load the bot's identity via the WebEx API.
        /// MUST be called by BotBuilder bots in order to filter messages sent by the bot.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task GetIdentityAsync()
        {
            await _api.GetMeAsync().ContinueWith(task => { Identity = task.Result.Data; }, TaskScheduler.Current).ConfigureAwait(false);
        }

        /// <summary>
        /// Clears out and resets all the webhook subscriptions currently associated with this application.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ResetWebhookSubscriptions()
        {
            await _api.ListWebhooksAsync().ContinueWith(
                async task =>
            {
                for (var i = 0; i < task.Result.Data.ItemCount; i++)
                {
                    await _api.DeleteWebhookAsync(task.Result.Data.Items[i]).ConfigureAwait(false);
                }
            }, TaskScheduler.Current).ConfigureAwait(false);
        }

        /// <summary>
        /// Register a webhook subscription with Webex Teams to start receiving message events.
        /// </summary>
        /// <param name="webhookPath">The path of the webhook endpoint like '/api/messages'.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task RegisterWebhookSubscriptionAsync(string webhookPath)
        {
            var webHookName = _config.WebhookName ?? "Botkit Firehose";

            await _api.ListWebhooksAsync().ContinueWith(
                async task =>
            {
                string hookId = null;

                for (var i = 0; i < task.Result.Data.ItemCount; i++)
                {
                    if (task.Result.Data.Items[i].Name == webHookName)
                    {
                        hookId = task.Result.Data.Items[i].Id;
                    }
                }

                var hookUrl = "https://" + _config.PublicAddress + webhookPath;

                if (hookId != null)
                {
                    await _api.UpdateWebhookAsync(hookId, webHookName, new Uri(hookUrl), _config.Secret).ConfigureAwait(false);
                }
                else
                {
                    await _api.CreateWebhookAsync(webHookName, new Uri(hookUrl), EventResource.All, EventType.All, null, _config.Secret).ConfigureAwait(false);
                }
            }, TaskScheduler.Current).ConfigureAwait(false);
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
                var personIdOrEmail = activity.GetChannelData<WebhookEventData>()?.MessageData.PersonEmail != null
                                    ? activity.GetChannelData<WebhookEventData>()?.MessageData.PersonEmail
                                    : activity.Recipient.Id;

                var responseId = await _webexApi.CreateMessageAsync(personIdOrEmail, activity.Text).ConfigureAwait(false);
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
                await _api.DeleteMessageAsync(reference.ActivityId, cancellationToken).ConfigureAwait(false);
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
        public async Task ProcessAsync(HttpRequest request, HttpResponse response, IBot bot, CancellationToken cancellationToken = default(CancellationToken))
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

            response.StatusCode = 200;
            await response.WriteAsync(string.Empty, cancellationToken).ConfigureAwait(false);

            WebhookEventData payload;
            using (var bodyStream = new StreamReader(request.Body))
            {
                payload = JsonConvert.DeserializeObject<WebhookEventData>(bodyStream.ReadToEnd());
            }

            if (!string.IsNullOrWhiteSpace(_config.Secret))
            {
                var json = JsonConvert.SerializeObject(payload);

                if (!ValidateSignature(_config.Secret, request, json))
                {
                    throw new Exception("WARNING: Webhook received message with invalid signature. Potential malicious behavior!");
                }
            }

            var activity = payload.Resource == EventResource.Message && payload.EventType == EventType.Created
                ? await DecryptedMessageToActivityAsync(payload).ConfigureAwait(false)
                : PayloadToActivity(payload);

            using (var context = new TurnContext(this, activity))
            {
                await RunPipelineAsync(context, bot.OnTurnAsync, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Validates the local secret against the one obtained from the request header.
        /// </summary>
        /// <param name="secret">The local stored secret.</param>
        /// <param name="request">The <see cref="HttpRequest"/> with the signature.</param>
        /// <param name="json">The serialized payload to be use for comparison.</param>
        /// <returns>The result of the comparison between the signature in the request and hashed json.</returns>
        private static bool ValidateSignature(string secret, HttpRequest request, string json)
        {
            var signature = request.Headers.ContainsKey("x-spark-signature")
                ? request.Headers["x-spark-signature"].ToString().ToUpperInvariant()
                : throw new Exception("HttpRequest is missing \"x-spark-signature\"");

            #pragma warning disable CA5350 // Webex API uses SHA1 as cryptographic algorithm.
            using (var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(secret)))
            {
                var hashArray = hmac.ComputeHash(Encoding.UTF8.GetBytes(json));
                var hash = BitConverter.ToString(hashArray).Replace("-", string.Empty).ToUpperInvariant();

                return signature == hash;
            }
            #pragma warning restore CA5350 // Webex API uses SHA1 as cryptographic algorithm.
        }

        /// <summary>
        /// Creates a <see cref="Activity"/> using the body of a request.
        /// </summary>
        /// <param name="payload">The payload obtained from the body of the request.</param>
        /// <returns>An <see cref="Activity"/> object.</returns>
        private Activity PayloadToActivity(WebhookEventData payload)
        {
            if (payload == null)
            {
                return null;
            }

            var activity = new Activity
            {
                Id = payload.Id,
                Timestamp = new DateTime(),
                ChannelId = "webex",
                Conversation = new ConversationAccount
                {
                    Id = payload.MessageData.SpaceId,
                },
                From = new ChannelAccount
                {
                    Id = payload.ActorId,
                },
                Recipient = new ChannelAccount
                {
                    Id = Identity.Id,
                },
                ChannelData = payload,
                Type = ActivityTypes.Event,
            };

            return activity;
        }

        /// <summary>
        /// Converts a decrypted <see cref="Message"/> into an <see cref="Activity"/>.
        /// </summary>
        /// <param name="payload">The payload obtained from the body of the request.</param>
        /// <returns>An <see cref="Activity"/> object.</returns>
        private async Task<Activity> DecryptedMessageToActivityAsync(WebhookEventData payload)
        {
            if (payload == null)
            {
                return null;
            }

            Message decryptedMessage = (await _api.GetMessageAsync(payload.MessageData.Id).ConfigureAwait(false)).GetData();
            var activity = new Activity
            {
                Id = decryptedMessage.Id,
                Timestamp = new DateTime(),
                ChannelId = "webex",
                Conversation = new ConversationAccount
                {
                    Id = decryptedMessage.SpaceId,
                },
                From = new ChannelAccount
                {
                    Id = decryptedMessage.PersonId,
                    Name = decryptedMessage.PersonEmail,
                },
                Recipient = new ChannelAccount
                {
                    Id = Identity.Id,
                },
                Text = decryptedMessage.Text,
                ChannelData = decryptedMessage,
                Type = ActivityTypes.Message,
            };

            // this is the bot speaking
            if (activity.From.Id == Identity.Id)
            {
                activity.Type = ActivityTypes.Event;
            }

            if (decryptedMessage.HasHtml)
            {
                var pattern = new Regex($"^(<p>)?<spark-mention .*?data-object-id=\"{Identity.Id}\".*?>.*?</spark-mention>");
                if (!decryptedMessage.Html.Equals(pattern))
                {
                    // this should look like ciscospark://us/PEOPLE/<id string>
                    var match = Regex.Match(Identity.Id, "/ciscospark://.*/(.*)/im");
                    pattern = new Regex($"^(<p>)?<spark-mention .*?data-object-id=\"{match.Captures[1]}\".*?>.*?</spark-mention>");
                }

                var action = decryptedMessage.Html.Replace(pattern.ToString(), string.Empty);

                // Strip the remaining HTML tags and replace the message text with the the HTML version
                activity.Text = action.Replace("/<.*?>/img", string.Empty).Trim();
            }
            else
            {
                var pattern = new Regex("^" + Identity.DisplayName + "\\s+");
                activity.Text = activity.Text.Replace(pattern.ToString(), string.Empty);
            }

            return activity;
        }
    }
}
