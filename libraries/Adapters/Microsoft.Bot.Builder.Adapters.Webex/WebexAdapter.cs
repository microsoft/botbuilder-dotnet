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
using Microsoft.Bot.Builder.BotKit.Core;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Thrzn41.WebexTeams.Version1;

namespace Microsoft.Bot.Builder.Adapters.Webex
{
    public class WebexAdapter : BotAdapter
    {
        /// <summary>
        /// Name used by Botkit plugin loader.
        /// </summary>
        public const string Name = "Webex Adapter";

        private readonly IWebexAdapterOptions config;

        private readonly TeamsAPIClient api;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebexAdapter"/> class.
        /// Creates a Webex adapter. See [WebexAdapterOptions] for a full definition of the allowed parameters.
        /// </summary>
        /// <param name="config">An object containing API credentials, a webhook verification token and other options.</param>
        public WebexAdapter(IWebexAdapterOptions config)
            : base()
        {
            this.config = config;

            if (this.config.AccessToken != null)
            {
                //this.api = new TeamsAPIClient(this.config.AccessToken, null, new Func<TeamsResultInfo>);

                if (this.api == null)
                {
                    throw new Exception("Could not create the Webex Teams API client");
                }
            }
            else
            {
                // error: access_token required to create controller
            }

            if (this.config.PublicAdress != null)
            {
                var endpoint = new Uri(this.config.PublicAdress);

                if (endpoint.Host != null)
                {
                    this.config.PublicAdress = endpoint.Host + endpoint.Port;
                }
                else
                {
                    // Could not determine hostname of public address
                }
            }
            else
            {
                // error: public_address parameter required to receive webhooks
            }

            if (this.config.Secret == null)
            {
                // error: WARNING: No secret specified. Source of incoming webhooks will not be validated. https://developer.webex.com/webhooks-explained.html#auth
            }

            /** middlewares **/
        }

        /// <summary>
        /// Gets a customized BotWorker.
        /// </summary>
        /// <value>A customized BotWorker object that exposes additional utility methods.</value>
        public WebexBotWorker BotkitWorker { get; private set; }

        /// <summary>
        /// Gets the identity of the bot.
        /// </summary>
        private Person Identity { get; set; }

        /// <summary>
        /// Load the bot's identity via the Webex API.
        /// MUST be called by BotBuilder bots in order to filter messages sent by the bot.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task GetIdentityAsync()
        {
            await this.api.GetMeAsync().ContinueWith((task) => { this.Identity = task.Result.Data; });
        }

        /// <summary>
        /// Botkit-only: Initialization function called automatically when used with Botkit.
        /// </summary>
        /// <param name="botkit">A botkit object.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task Init(Botkit botkit)
        {
            // when the bot is ready, register the webhook subscription with the Webex API
            botkit.AddDep("webex-identity");

            await this.GetIdentityAsync().ContinueWith((task) => botkit.CompleteDep("webex-identity"));

            botkit.Ready(() => { (botkit.Adapter as WebexAdapter).RegisterWebhookSubscription(botkit.GetConfig("webhook_uri").ToString()); });
        }

        /// <summary>
        /// Clears out and resets all the webhook subscriptions currently associated with this application.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ResetWebhookSubscriptions()
        {
            await this.api.ListWebhooksAsync().ContinueWith(async (task) =>
            {
                for (int i = 0; i < task.Result.Data.ItemCount; i++)
                {
                    await this.api.DeleteWebhookAsync(task.Result.Data.Items[i]);
                }
            });
        }

        /// <summary>
        /// Register a webhook subscription with Webex Teams to start receiving message events.
        /// </summary>
        /// <param name="webhookPath">The path of the webhook endpoint like `/api/messages`.</param>
        public void RegisterWebhookSubscription(string webhookPath)
        {
            var webHookName = this.config.WebhookName ?? "Botkit Firehose";

            this.api.ListWebhooksAsync().ContinueWith(async (task) =>
            {
                string hookId = null;

                for (var i = 0; i < task.Result.Data.ItemCount; i++)
                {
                    if (task.Result.Data.Items[i].Name == webHookName)
                    {
                        hookId = task.Result.Data.Items[i].Id;
                    }
                }

                var hookURL = "https://" + this.config.PublicAdress + webhookPath;

                if (hookId != null)
                {
                    await this.api.UpdateWebhookAsync(hookId, webHookName, new Uri(hookURL), this.config.Secret);
                }
                else
                {
                    await this.api.CreateWebhookAsync(webHookName, new Uri(hookURL), EventResource.All, EventType.All);
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
                    var personIDorEmail = ((activity.ChannelData as dynamic).toPersonEmail != null) ? (activity.ChannelData as dynamic).toPersonEmail : activity.Recipient.Id;
                    var text = (activity.ChannelData != null) ? (activity.ChannelData as dynamic).markdown : activity.Text;
                    Message webexResponse = await this.api.CreateDirectMessageAsync(personIDorEmail, text);
                    var response = new ResourceResponse(webexResponse.Id);
                    responses.Add(response);
                }
                else
                {
                    // not type message
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
        /// <param name="turnContext">A TurnContext representing the current incoming message and environment.</param>
        /// <param name="reference">An object in the form `{activityId: -id of message to delete-, conversation: { id: -id of Webex channel>}}`.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken)
        {
            if (reference.ActivityId != null)
            {
                await this.api.DeleteMessageAsync(reference.ActivityId, default(CancellationToken));
            }
        }

        /// <summary>
        /// Standard BotBuilder adapter method for continuing an existing conversation based on a conversation reference.
        /// </summary>
        /// <param name="reference">A conversation reference to be applied to future messages.</param>
        /// <param name="logic">A bot logic function that will perform continuing action in the form 'async(context) => { ... }'.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ContinueConversationAsync(ConversationReference reference, BotCallbackHandler logic)
        {
            var request = reference.GetContinuationActivity().ApplyConversationReference(reference, true);

            var context = new TurnContext(this, request);

            await this.RunPipelineAsync(context, logic, default(CancellationToken));
        }

        /// <summary>
        /// Accept an incoming webhook request and convert it into a TurnContext which can be processed by the bot's logic.
        /// </summary>
        /// <param name="request">A request object from Restify or Express.</param>
        /// <param name="response">A response object from Restify or Express.</param>
        /// <param name="bot">A bot with logic function in the form `async(context) => { ... }`.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ProcessAsync(HttpRequest request, HttpResponse response, IBot bot, CancellationToken cancellationToken = default(CancellationToken))
        {
            response.StatusCode = 200;
            await response.WriteAsync(string.Empty);

            var bodyStream = new StreamReader(request.Body);
            dynamic payload = JsonConvert.DeserializeObject(bodyStream.ReadToEnd());

            Activity activity;

            if (this.config.Secret != null)
            {
                var signature = request.Headers["x-spark-signature"];
                var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(this.config.Secret));
                var hash = hmac.ComputeHash(payload);

                if (signature != hash)
                {
                    throw new Exception("WARNING: Webhook received message with invalid signature. Potential malicious behavior!");
                }
            }

            if (payload.resouece == "messages" && payload["event"] == "created")
            {
                Message decryptedMessage = await this.api.GetMessageAsync(payload.data);
                activity = new Activity()
                {
                    Id = decryptedMessage.Id,
                    Timestamp = new DateTime(),
                    ChannelId = "webex",
                    Conversation = new ConversationAccount()
                    {
                        Id = (decryptedMessage as dynamic).roomId, // try some other property from Message
                    },
                    From = new ChannelAccount()
                    {
                        Id = decryptedMessage.PersonId,
                        Name = decryptedMessage.PersonEmail,
                    },
                    Recipient = new ChannelAccount()
                    {
                        Id = this.Identity.Id,
                    },
                    Text = decryptedMessage.Text,
                    ChannelData = decryptedMessage,
                    Type = ActivityTypes.Message,
                };

                // this is the bot speaking
                if (activity.From.Id == this.Identity.Id)
                {
                    (activity.ChannelData as dynamic).botkitEventType = "self_message";
                    activity.Type = ActivityTypes.Event;
                }

                if (decryptedMessage.HasHtml)
                {
                    var pattern = new Regex("^(<p>)?<spark-mention .*?data-object-id=" + this.Identity.Id + ".*?>.*?</spark-mention>");
                    if (!decryptedMessage.Html.Equals(pattern))
                    {
                        var encodedId = this.Identity.Id;
                        //var decoded = // Needs decoding?

                        // this should look like ciscospark://us/PEOPLE/<id string>
                        Match match = Regex.Match(encodedId, "/ciscospark://.*/(.*)/im");
                        pattern = new Regex("^(<p>)?<spark-mention .*?data-object-id=" + match.Captures[1] + ".*?>.*?</spark-mention>");
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
                    var pattern = new Regex("^" + this.Identity.DisplayName + "\\s+");
                    activity.Text = activity.Text.Replace(pattern.ToString(), string.Empty);
                }

                var context = new TurnContext(this, activity);

                await this.RunPipelineAsync(context, bot.OnTurnAsync, default(CancellationToken));
            }
            else
            {
                // type == payload.resource + '.' + payload.event
                // memberships.deleted for example
                // payload.data contains stuff
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
                        Id = this.Identity.Id,
                    },
                    ChannelData = payload,
                    Type = ActivityTypes.Event,
                };

                (activity.ChannelData as dynamic).botkitEventType = payload.resource + "." + payload["event"];

                var context = new TurnContext(this, activity);

                await this.RunPipelineAsync(context, bot.OnTurnAsync, default(CancellationToken));
            }
        }
    }
}
