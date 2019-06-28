// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using SlackAPI;

namespace Microsoft.BotBuilder.Adapters.Slack
{
    public class SlackAdapter : BotAdapter, IBotFrameworkHttpAdapter
    {
        /// <summary>
        /// Name used by Botkit plugin loader.
        /// </summary>
        public const string NAME = "Slack Adapter";

        private readonly ISlackAdapterOptions options;
        private readonly SlackTaskClient slack;
        private readonly string slackOAuthURL = "https://slack.com/oauth/authorize?client_id=";
        private string identity;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlackAdapter"/> class.
        /// Create a Slack adapter.
        /// </summary>
        /// <param name="options">An object containing API credentials, a webhook verification token and other options.</param>
        public SlackAdapter(ISlackAdapterOptions options)
            : base()
        {
            this.options = options;

            if (this.options.VerificationToken != null && this.options.ClientSigningSecret != null)
            {
                string warning =
                    "****************************************************************************************" +
                    "* WARNING: Your bot is operating without recommended security mechanisms in place.     *" +
                    "* Initialize your adapter with a clientSigningSecret parameter to enable               *" +
                    "* verification that all incoming webhooks originate with Slack:                        *" +
                    "*                                                                                      *" +
                    "* var adapter = new SlackAdapter({clientSigningSecret: <my secret from slack>});       *" +
                    "*                                                                                      *" +
                    "****************************************************************************************" +
                    ">> Slack docs: https://api.slack.com/docs/verifying-requests-from-slack";

                throw new Exception(warning + Environment.NewLine + "Required: include a verificationToken or clientSigningSecret to verify incoming Events API webhooks");
            }

            this.slack = new SlackTaskClient(this.options.BotToken);
            this.LoginWithSlack().Wait();
        }

        /// <summary>
        /// Gets a customized BotWorker.
        /// </summary>
        /// <value>
        /// A customized BotWorker object that exposes additional utility methods.
        /// </value>
        public SlackBotWorker BotkitWorker { get; private set; }

        /// <summary>
        /// Get a Slack API client with the correct credentials based on the team identified in the incoming activity.
        /// This is used by many internal functions to get access to the Slack API, and is exposed as `bot.api` on any bot worker instances.
        /// </summary>
        /// <param name="activity">An Activity object.</param>
        /// <returns>Returns an instance of the Slack API.</returns>
        public async Task<SlackTaskClient> GetAPIAsync(Activity activity)
        {
            if (this.slack != null)
            {
                return this.slack;
            }

            if (activity.Conversation.Properties["team"] == null)
            {
                throw new Exception($"Unable to create API based on activity:{activity}");
            }

            var token = await this.options.GetTokenForTeam(activity.Conversation.Properties["team"].ToString());
            return string.IsNullOrEmpty(token) ? new SlackTaskClient(token) : throw new Exception("Missing credentials for team.");
        }

        /// <summary>
        /// Get the bot user id associated with the team on which an incoming activity originated. This is used internally by the SlackMessageTypeMiddleware to identify direct_mention and mention events.
        /// In single-team mode, this will pull the information from the Slack API at launch.
        /// In multi-team mode, this will use the `getBotUserByTeam` method passed to the constructor to pull the information from a developer-defined source.
        /// </summary>
        /// <param name="activity">An Activity.</param>
        /// <returns>The identity of the bot's user.</returns>
        public async Task<string> GetBotUserByTeamAsync(Activity activity)
        {
            if (!string.IsNullOrEmpty(this.identity))
            {
                return this.identity;
            }

            if (activity.Conversation.Properties["team"] == null)
            {
                return null;
            }

            // multi-team mode
            var userID = await this.options.GetBotUserByTeam(activity.Conversation.Properties["team"].ToString());
            return !string.IsNullOrEmpty(userID) ? userID : throw new Exception("Missing credentials for team.");
        }

        /// <summary>
        /// Get the oauth link for this bot, based on the clientId and scopes passed in to the constructor.
        /// </summary>
        /// <returns>A url pointing to the first step in Slack's oauth flow.</returns>
        public string GetInstallLink()
        {
            return (!string.IsNullOrEmpty(this.options.ClientId) && this.options.Scopes.Length > 0)
                ? this.slackOAuthURL + this.options.ClientId + "&scope=" + string.Join(",", this.options.Scopes)
                : throw new Exception("getInstallLink() cannot be called without clientId and scopes in adapter options.");
        }

        /// <summary>
        /// Validates an oauth code sent by Slack during the install process.
        /// </summary>
        /// <param name="code">The value found in `req.query.code` as part of Slack's response to the oauth flow.</param>
        /// <returns>The access token.</returns>
        public async Task<AccessTokenResponse> ValidateOauthCodeAsync(string code)
        {
            var helpers = new SlackClientHelpers();
            var results = await helpers.GetAccessTokenAsync(this.options.ClientId, this.options.ClientSecret, this.options.RedirectUri, code);
            return results.ok ? results : throw new Exception(results.error);
        }

        /// <summary>
        /// Formats a BotBuilder activity into an outgoing Slack message.
        /// </summary>
        /// <param name="activity">A BotBuilder Activity object.</param>
        /// <returns>A Slack message object with {text, attachments, channel, thread ts} as well as any fields found in activity.channelData.</returns>
        public NewSlackMessage ActivityToSlack(Activity activity)
        {
            NewSlackMessage message = new NewSlackMessage();

            if (activity.Timestamp != null)
            {
                message.ts = activity.Timestamp.Value.DateTime;
            }

            message.text = activity.Text;

            foreach (Microsoft.Bot.Schema.Attachment att in activity.Attachments)
            {
                SlackAPI.Attachment newAttachment = new SlackAPI.Attachment()
                {
                    author_name = att.Name,
                    thumb_url = att.ThumbnailUrl,
                };
                message.attachments.Add(newAttachment);
            }

            message.channel = activity.Conversation.Id;

            if (activity.Conversation.Properties["thread_ts"].ToString() != string.Empty)
            {
                message.ThreadTS = activity.Conversation.Properties["thread_ts"].ToString();
            }

            // if channelData is specified, overwrite any fields in message object
            if (activity.ChannelData != null)
            {
                try
                {
                    // Try a straight up cast
                    message = activity.ChannelData as NewSlackMessage;
                }
                catch (InvalidCastException)
                {
                    foreach (var property in message.GetType().GetFields())
                    {
                        string name = property.Name;
                        var value = (activity.ChannelData as dynamic)[name];
                        if (value != null)
                        {
                            message.GetType().GetField(name).SetValue(message, value);
                        }
                    }
                }
            }

            // should this message be sent as an ephemeral message
            if (message.Ephemeral != null)
            {
                message.user = activity.Recipient.Id;
            }

            if (message.IconUrl != null || message.icons?.status_emoji != null || message.username != null)
            {
                message.AsUser = false;
            }

            return message;
        }

        /// <summary>
        /// Standard BotBuilder adapter method to send a message from the bot to the messaging API.
        /// </summary>
        /// <param name="turnContext">A TurnContext representing the current incoming message and environment.</param>
        /// <param name="activities">An array of outgoing activities to be sent back to the messaging API.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A resource response.</returns>
        public override async Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)
        {
            var responses = new List<ResourceResponse>();
            for (var i = 0; i < activities.Length; i++)
            {
                Activity activity = activities[i];
                if (activity.Type == ActivityTypes.Message)
                {
                    NewSlackMessage message = this.ActivityToSlack(activity);

                    try
                    {
                        SlackTaskClient slack = await this.GetAPIAsync(turnContext.Activity);
                        SlackResponse responseInString;

                        var data = new NameValueCollection();
                        data["token"] = this.options.BotToken;
                        data["channel"] = message.channel;
                        data["text"] = message.text;
                        data["thread_ts"] = message.ThreadTS;

                        var client = new WebClient();

                        string url = message.Ephemeral != null
                            ? "https://slack.com/api/chat.postEphemeral"
                            : "https://slack.com/api/chat.postMessage";

                        var response = client.UploadValues(url, "POST", data);
                        responseInString = JsonConvert.DeserializeObject<SlackResponse>(Encoding.UTF8.GetString(response));

                        if (responseInString.Ok)
                        {
                            ActivityResourceResponse resourceResponse = new ActivityResourceResponse()
                            {
                                Id = responseInString.TS,
                                ActivityID = responseInString.TS,
                                Conversation = new ConversationAccount()
                                {
                                    Id = responseInString.Channel,
                                },
                            };
                            responses.Add(resourceResponse as ResourceResponse);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
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
        public override async Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            if (activity.Id != null && activity.Conversation != null)
            {
                NewSlackMessage message = this.ActivityToSlack(activity);
                SlackTaskClient slack = await this.GetAPIAsync(activity);
                var results = await slack.UpdateAsync(activity.Timestamp.ToString(), activity.ChannelId, message.text);
                if (!results.ok)
                {
                    throw new Exception($"Error updating activity on Slack:{results}");
                }
            }
            else
            {
                throw new Exception("Cannot update activity: activity is missing id.");
            }

            return new ResourceResponse()
            {
                Id = activity.Id,
            };
        }

        /// <summary>
        /// Standard BotBuilder adapter method to delete a previous message.
        /// </summary>
        /// <param name="turnContext">A TurnContext representing the current incoming message and environment.</param>
        /// <param name="reference">An object in the form "{activityId: `id of message to delete`, conversation: { id: `id of slack channel`}}".</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken)
        {
            if (reference.ActivityId != null && reference.Conversation != null)
            {
                SlackTaskClient slack = await this.GetAPIAsync(turnContext.Activity);
                var results = await slack.DeleteMessageAsync(reference.ChannelId, turnContext.Activity.Timestamp.Value.DateTime);
            }
            else
            {
                throw new Exception("Cannot delete activity: reference is missing activityId.");
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
            var request = reference.GetContinuationActivity().ApplyConversationReference(reference, true); // TODO: check on this

            using (TurnContext context = new TurnContext(this, request))
            {
                await this.RunPipelineAsync(context, logic, default(CancellationToken));
            }
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
            try
            {
                // Create an Activity based on the incoming message from Slack.
                // There are a few different types of event that Slack might send.
                StreamReader sr = new StreamReader(request.Body);
                dynamic slackEvent = JsonConvert.DeserializeObject(sr.ReadToEnd());

                if (slackEvent.type == "url_verification")
                {
                    response.StatusCode = 200;
                    response.ContentType = "text/plain";
                    string text = slackEvent.challenge.ToString();
                    await response.WriteAsync(text);
                }

                if (this.VerifySignature(request, response))
                {
                    if (slackEvent.payload != null)
                    {
                        // handle interactive_message callbacks and block_actions
                        slackEvent = JsonConvert.ToString(slackEvent.payload);
                        if (this.options.VerificationToken != null && slackEvent.token != this.options.VerificationToken)
                        {
                            response.StatusCode = 403;
                        }
                        else
                        {
                            Activity activity = new Activity()
                            {
                                Timestamp = default(DateTime),
                                ChannelId = "slack",
                                Conversation = new ConversationAccount()
                                {
                                    Id = slackEvent.Channel.Id,
                                },
                                From = new ChannelAccount()
                                {
                                    Id = (slackEvent.BotId != null) ? slackEvent.BotId : slackEvent.User.Id,
                                },
                                Recipient = new ChannelAccount()
                                {
                                    Id = null,
                                },
                                ChannelData = slackEvent,
                                Type = ActivityTypes.Event,
                            };

                            // Extra fields that do not belong to activity
                            activity.Conversation.Properties["thread_ts"] = slackEvent["event"].thread_ts;
                            activity.Conversation.Properties["team"] = slackEvent.Team.Id;

                            // this complains because of extra fields in conversation
                            activity.Recipient.Id = await this.GetBotUserByTeamAsync(activity);

                            // create a conversation reference
                            using (var context = new TurnContext(this, activity))
                            {
                                context.TurnState.Add("httpStatus", "200");

                                await this.RunPipelineAsync(context, bot.OnTurnAsync, default(CancellationToken));

                                // send http response back
                                response.StatusCode = Convert.ToInt32(context.TurnState.Get<string>("httpStatus"));
                                response.ContentType = "text/plain";
                                string text = (context.TurnState.Get<object>("httpBody") != null) ? context.TurnState.Get<object>("httpBody").ToString() : string.Empty;
                                await response.WriteAsync(text);
                            }
                        }
                    }
                    else if (slackEvent.type == "event_callback")
                    {
                        // this is an event api post
                        if (this.options.VerificationToken != null && slackEvent.token != this.options.VerificationToken)
                        {
                            response.StatusCode = 403;
                            response.ContentType = "text/plain";
                            string text = $"Rejected due to mismatched verificationToken:{slackEvent}";
                            await response.WriteAsync(text);
                        }
                        else
                        {
                            Activity activity = new Activity()
                            {
                                Id = slackEvent["event"].ts,
                                Timestamp = default(DateTime),
                                ChannelId = "slack",
                                Conversation = new ConversationAccount()
                                {
                                    Id = slackEvent["event"].channel,
                                },
                                From = new ChannelAccount()
                                {
                                    Id = (slackEvent["event"].bot_id != null) ? slackEvent["event"].bot_id : slackEvent["event"].user,
                                },
                                Recipient = new ChannelAccount()
                                {
                                    Id = null,
                                },
                                ChannelData = slackEvent["event"],
                                Text = null,
                                Type = ActivityTypes.Event,
                            };

                            // Extra field that doesn't belong to activity
                            activity.Conversation.Properties["thread_ts"] = slackEvent["event"].thread_ts;

                            // this complains because of extra fields in conversation
                            activity.Recipient.Id = await this.GetBotUserByTeamAsync(activity);

                            // Normalize the location of the team id
                            (activity.ChannelData as dynamic).team = slackEvent.team_id;

                            // add the team id to the conversation record
                            activity.Conversation.Properties["team"] = (activity.ChannelData as dynamic).team;

                            // If this is conclusively a message originating from a user, we'll mark it as such
                            if (slackEvent["event"].type == "message" && slackEvent["event"].subtype == null)
                            {
                                activity.Type = ActivityTypes.Message;
                                activity.Text = slackEvent["event"].text;
                            }

                            // create a conversation reference
                            using (TurnContext context = new TurnContext(this, activity))
                            {
                                context.TurnState.Add("httpStatus", "200");

                                await this.RunPipelineAsync(context, bot.OnTurnAsync, default(CancellationToken));

                                // send http response back
                                response.StatusCode = Convert.ToInt32(context.TurnState.Get<string>("httpStatus"));
                                response.ContentType = "text/plain";
                                string text = (context.TurnState.Get<object>("httpBody") != null) ? context.TurnState.Get<object>("httpBody").ToString() : string.Empty;
                                await response.WriteAsync(text);
                            }
                        }
                    }
                    else if (slackEvent.Command != null)
                    {
                        if (this.options.VerificationToken != null && slackEvent.Token != this.options.VerificationToken)
                        {
                            response.StatusCode = 403;
                        }
                        else
                        {
                            // this is a slash command
                            Activity activity = new Activity()
                            {
                                Id = slackEvent.TriggerId,
                                Timestamp = default(DateTime),
                                ChannelId = "slack",
                                Conversation = new ConversationAccount()
                                {
                                    Id = slackEvent.ChannelId,
                                },
                                From = new ChannelAccount()
                                {
                                    Id = slackEvent.UserId,
                                },
                                Recipient = new ChannelAccount()
                                {
                                    Id = null,
                                },
                                ChannelData = slackEvent,
                                Text = slackEvent.text,
                                Type = ActivityTypes.Event,
                            };

                            activity.Recipient.Id = await this.GetBotUserByTeamAsync(activity);

                            // Normalize the location of the team id
                            (activity.ChannelData as dynamic).team = slackEvent.TeamId;

                            // add the team id to the conversation record
                            activity.Conversation.Properties["team"] = (activity.ChannelData as dynamic).team;

                            (activity.ChannelData as dynamic).BotkitEventType = "slash_command";

                            // create a conversation reference
                            using (TurnContext context = new TurnContext(this, activity))
                            {
                                context.TurnState.Add("httpStatus", "200");

                                await this.RunPipelineAsync(context, bot.OnTurnAsync, default(CancellationToken));

                                // send http response back
                                response.StatusCode = Convert.ToInt32(context.TurnState.Get<string>("httpStatus"));
                                response.ContentType = "text/plain";
                                string text = (context.TurnState.Get<object>("httpBody") != null) ? context.TurnState.Get<object>("httpBody").ToString() : string.Empty;
                                await response.WriteAsync(text);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Something went wrong: {ex.Message}");
            }
        }

        private async Task LoginWithSlack()
        {
            if (this.options.BotToken != null)
            {
                AuthTestResponse response = await this.slack.TestAuthAsync();
                this.identity = response.user_id;
            }
            else
            {
                if (string.IsNullOrEmpty(this.options.ClientId) || string.IsNullOrEmpty(this.options.ClientSecret) ||
                 string.IsNullOrEmpty(this.options.RedirectUri) || this.options.Scopes.Length > 0)
                {
                    throw new Exception("Missing Slack API credentials! Provide clientId, clientSecret, scopes and redirectUri as part of the SlackAdapter options.");
                }
            }
        }

        /// <summary>
        /// Verify the signature of an incoming webhook request as originating from Slack.
        /// </summary>
        /// <returns>If signature is valid, returns true. Otherwise, sends a 401 error status via http response and then returns false.</returns>
        private bool VerifySignature(HttpRequest request, HttpResponse response)
        {
            if (this.options.ClientSigningSecret != null && request.Body != null)
            {
                var timestamp = request.Headers;
                var body = request.Body;

                object[] signature = { "v0", timestamp.ToString(), body.ToString() };

                string baseString = string.Join(":", signature);

                var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(this.options.ClientSigningSecret));

                var hash = "v0=" + hmac.ComputeHash(Encoding.UTF8.GetBytes(baseString));

                var retrievedSignature = request.Headers["X-Slack-Signature"];

                // Compare the hash of the computed signature with the retrieved signature with a secure hmac compare function
                bool signatureIsValid = string.Equals(hash, retrievedSignature);

                // replace direct compare with the hmac result
                if (!signatureIsValid)
                {
                    response.StatusCode = 401;
                    return false;
                }
            }

            return true;
        }
    }
}
