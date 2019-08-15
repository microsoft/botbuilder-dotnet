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

        private readonly TeamsAPIClient _api;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebexAdapter"/> class.
        /// Creates a Webex adapter. See <see cref="IWebexAdapterOptions"/> for a full definition of the allowed parameters.
        /// </summary>
        /// <param name="config">An object containing API credentials, a webhook verification token and other options.</param>
        public WebexAdapter(IWebexAdapterOptions config)
            : base()
        {
            _config = config;

            if (_config.AccessToken != null)
            {
                _api = TeamsAPI.CreateVersion1Client(config.AccessToken);

                if (_api == null)
                {
                    throw new Exception("Could not create the Webex Teams API client");
                }
            }
            else
            {
                throw new Exception("AccessToken required to create controller");
            }

            if (_config.PublicAddress != null)
            {
                var endpoint = new Uri(_config.PublicAddress);

                if (string.IsNullOrWhiteSpace(endpoint.Host))
                {
                    _config.PublicAddress = endpoint.Host;
                }
                else
                {
                    throw new Exception("Could not determine hostname of public address");
                }
            }
            else
            {
                throw new Exception("PublicAddress parameter required to receive webhooks");
            }
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
            await _api.GetMeAsync().ContinueWith((task) => { Identity = task.Result.Data; });
        }

        /// <summary>
        /// Clears out and resets all the webhook subscriptions currently associated with this application.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ResetWebhookSubscriptions()
        {
            await _api.ListWebhooksAsync().ContinueWith(async (task) =>
            {
                for (int i = 0; i < task.Result.Data.ItemCount; i++)
                {
                    await _api.DeleteWebhookAsync(task.Result.Data.Items[i]);
                }
            });
        }

        /// <summary>
        /// Register a webhook subscription with Webex Teams to start receiving message events.
        /// </summary>
        /// <param name="webhookPath">The path of the webhook endpoint like '/api/messages'.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task RegisterWebhookSubscriptionAsync(string webhookPath)
        {
            var webHookName = _config.WebhookName ?? "Botkit Firehose";

            await _api.ListWebhooksAsync().ContinueWith(async (task) =>
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
                    await _api.UpdateWebhookAsync(hookId, webHookName, new Uri(hookUrl), _config.Secret);
                }
                else
                {
                    await _api.CreateWebhookAsync(webHookName, new Uri(hookUrl), EventResource.All, EventType.All, null, _config.Secret);
                }
            });
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
            List<ResourceResponse> responses = new List<ResourceResponse>();
            for (int i = 0; i < activities.Length; i++)
            {
                var activity = activities[i];
                if (activity.Type.Equals(ActivityTypes.Message))
                {
                    // transform activity into the webex message format
                    var personIDorEmail = ((activity.ChannelData as dynamic)?.toPersonEmail != null) ? (activity.ChannelData as dynamic).toPersonEmail : activity.Recipient.Id;
                    var text = (activity.ChannelData != null) ? (activity.ChannelData as dynamic).markdown : activity.Text;
                    TeamsResult<Message> webexResponse = await _api.CreateDirectMessageAsync(personIDorEmail, text);
                    var response = new ResourceResponse(webexResponse.Data.Id);
                    responses.Add(response);
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
        public override async Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            // Webex adapter does not support updateActivity.
            return await Task.FromException<ResourceResponse>(new NotImplementedException("Webex adapter does not support updateActivity."));
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
            if (reference.ActivityId != null)
            {
                await _api.DeleteMessageAsync(reference.ActivityId, default(CancellationToken));
            }
        }

        /// <summary>
        /// Standard BotBuilder adapter method for continuing an existing conversation based on a conversation reference.
        /// </summary>
        /// <param name="reference">A <see cref="ConversationReference"/> to be applied to future messages.</param>
        /// <param name="logic">A bot logic function that will perform continuing action.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ContinueConversationAsync(ConversationReference reference, BotCallbackHandler logic)
        {
            var request = reference.GetContinuationActivity().ApplyConversationReference(reference, true);

            var context = new TurnContext(this, request);

            await RunPipelineAsync(context, logic, default(CancellationToken));
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
            response.StatusCode = 200;
            await response.WriteAsync(string.Empty);

            var bodyStream = new StreamReader(request.Body);
            dynamic payload = JsonConvert.DeserializeObject(bodyStream.ReadToEnd());

            var json = JsonConvert.SerializeObject(payload);

            if (!string.Equals(_config.Secret, string.Empty))
            {
                var signature = request.Headers["x-spark-signature"];

                using (var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(_config.Secret)))
                {
                    var hashArray = hmac.ComputeHash(Encoding.UTF8.GetBytes(json));

                    string hash = BitConverter.ToString(hashArray).Replace("-", string.Empty).ToLower();

                    if (!string.Equals(signature, hash))
                    {
                        throw new Exception("WARNING: Webhook received message with invalid signature. Potential malicious behavior!");
                    }
                }
            }

            Activity activity;
            if (payload.resource == "messages" && payload["event"] == "created")
            {
                Message decryptedMessage = (await _api.GetMessageAsync(payload.data.id.ToString())).GetData();
                activity = new Activity()
                {
                    Id = decryptedMessage.Id,
                    Timestamp = new DateTime(),
                    ChannelId = "webex",
                    Conversation = new ConversationAccount()
                    {
                        Id = (decryptedMessage as dynamic).SpaceId,
                    },
                    From = new ChannelAccount()
                    {
                        Id = decryptedMessage.PersonId,
                        Name = decryptedMessage.PersonEmail,
                    },
                    Recipient = new ChannelAccount()
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
                    (activity.ChannelData as dynamic).botkitEventType = "self_message";
                    activity.Type = ActivityTypes.Event;
                }

                if (decryptedMessage.HasHtml)
                {
                    var pattern = new Regex("^(<p>)?<spark-mention .*?data-object-id=\"" + Identity.Id + "\".*?>.*?</spark-mention>");
                    if (!decryptedMessage.Html.Equals(pattern))
                    {
                        var encodedId = Identity.Id;

                        // this should look like ciscospark://us/PEOPLE/<id string>
                        Match match = Regex.Match(encodedId, "/ciscospark://.*/(.*)/im");
                        pattern = new Regex("^(<p>)?<spark-mention .*?data-object-id=\"" + match.Captures[1] + "\".*?>.*?</spark-mention>");
                    }

                    var action = decryptedMessage.Html.Replace(pattern.ToString(), string.Empty);

                    // strip the remaining HTML tags
                    action = action.Replace("/<.*?>/img", string.Empty);

                    // strip remaining whitespace
                    action = action.Trim();

                    // replace the message text with the the HTML version
                    activity.Text = action;
                }
                else
                {
                    var pattern = new Regex("^" + Identity.DisplayName + "\\s+");
                    activity.Text = activity.Text.Replace(pattern.ToString(), string.Empty);
                }

                var context = new TurnContext(this, activity);

                await RunPipelineAsync(context, bot.OnTurnAsync, default(CancellationToken));
            }
            else
            {
                activity = new Activity()
                {
                    Id = payload.id,
                    Timestamp = new DateTime(),
                    ChannelId = "webex",
                    Conversation = new ConversationAccount()
                    {
                        Id = payload.data.roomId,
                    },
                    From = new ChannelAccount()
                    {
                        Id = payload.actorId,
                    },
                    Recipient = new ChannelAccount()
                    {
                        Id = Identity.Id,
                    },
                    ChannelData = payload,
                    Type = ActivityTypes.Event,
                };

                (activity.ChannelData as dynamic).botkitEventType = payload.resource + "." + payload["event"];

                var context = new TurnContext(this, activity);

                await RunPipelineAsync(context, bot.OnTurnAsync, default(CancellationToken));
            }
        }
    }
}
