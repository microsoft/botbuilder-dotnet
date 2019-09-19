// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using SlackAPI;

namespace Microsoft.Bot.Builder.Adapters.Slack
{
    public class SlackAdapter : BotAdapter, IBotFrameworkHttpAdapter
    {
        private const string PostMessageUrl = "https://slack.com/api/chat.postMessage";
        private const string PostEphemeralMessageUrl = "https://slack.com/api/chat.postEphemeral";
        private const string SlackOAuthUrl = "https://slack.com/oauth/authorize?client_id=";

        private readonly SlackAdapterOptions _options;
        private readonly SlackClientWrapper _slackClient;

        private string _identity;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlackAdapter"/> class.
        /// Create a Slack adapter.
        /// </summary>
        /// <param name="slackClient">An initialized instance of the SlackClientWrapper class.</param>
        /// <param name="options">An object containing API credentials, a webhook verification token and other options.</param>
        public SlackAdapter(SlackClientWrapper slackClient, SlackAdapterOptions options)
            : base()
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrWhiteSpace(_options.VerificationToken) && string.IsNullOrWhiteSpace(_options.ClientSigningSecret))
            {
                var warning =
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

            _slackClient = slackClient ?? throw new ArgumentNullException(nameof(slackClient));
            LoginWithSlack().Wait();
        }

        /// <summary>
        /// Get a Slack API client with the correct credentials based on the team identified in the incoming activity.
        /// This is used by many internal functions to get access to the Slack API, and is exposed as `bot.api` on any bot worker instances.
        /// </summary>
        /// <param name="activity">An Activity object.</param>
        /// <returns>Returns an instance of the Slack API.</returns>
        public async Task<SlackClientWrapper> GetAPIAsync(Activity activity)
        {
            if (_slackClient != null)
            {
                return _slackClient;
            }

            if (activity.Conversation.Properties["team"] == null)
            {
                throw new Exception($"Unable to create API based on activity:{activity}");
            }

            var token = await _options.GetTokenForTeam(activity.Conversation.Properties["team"].ToString()).ConfigureAwait(false);
            return string.IsNullOrWhiteSpace(token) ? new SlackClientWrapper(token) : throw new Exception("Missing credentials for team.");
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
            if (!string.IsNullOrWhiteSpace(_identity))
            {
                return _identity;
            }

            if (activity.Conversation.Properties["team"] == null)
            {
                return null;
            }

            // multi-team mode
            var userId = await _options.GetBotUserByTeam(activity.Conversation.Properties["team"].ToString()).ConfigureAwait(false);
            return !string.IsNullOrWhiteSpace(userId) ? userId : throw new Exception("Missing credentials for team.");
        }

        /// <summary>
        /// Get the oauth link for this bot, based on the clientId and scopes passed in to the constructor.
        /// </summary>
        /// <returns>A url pointing to the first step in Slack's oauth flow.</returns>
        public string GetInstallLink()
        {
            return (!string.IsNullOrWhiteSpace(_options.ClientId) && _options.GetScopes().Length > 0)
                ? SlackOAuthUrl + _options.ClientId + "&scope=" + string.Join(",", _options.GetScopes())
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
            var results = await helpers.GetAccessTokenAsync(_options.ClientId, _options.ClientSecret, _options.RedirectUri.AbsolutePath, code).ConfigureAwait(false);
            return results.ok ? results : throw new Exception(results.error);
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
                    NewSlackMessage message = SlackHelper.ActivityToSlack(activity);

                    SlackResponse responseInString;

                    var data = new NameValueCollection();
                    data["token"] = _options.BotToken;
                    data["channel"] = message.channel;
                    data["text"] = message.text;
                    data["thread_ts"] = message.ThreadTS;

                    byte[] response;
                    using (var client = new WebClient())
                    {
                        string url = !string.IsNullOrWhiteSpace(message.Ephemeral)
                            ? PostEphemeralMessageUrl
                            : PostMessageUrl;

                        response = await client.UploadValuesTaskAsync(url, "POST", data).ConfigureAwait(false);
                    }

                    responseInString = JsonConvert.DeserializeObject<SlackResponse>(Encoding.UTF8.GetString(response));

                    if (responseInString.Ok)
                    {
                        ActivityResourceResponse resourceResponse = new ActivityResourceResponse()
                        {
                            Id = responseInString.TS,
                            ActivityId = responseInString.TS,
                            Conversation = new ConversationAccount()
                            {
                                Id = responseInString.Channel,
                            },
                        };
                        responses.Add(resourceResponse as ResourceResponse);
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
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            if (activity.Id == null)
            {
                throw new ArgumentException(nameof(activity.Id));
            }

            if (activity.Conversation == null)
            {
                throw new ArgumentException(nameof(activity.Conversation));
            }

            var message = SlackHelper.ActivityToSlack(activity);
            var slack = await GetAPIAsync(activity).ConfigureAwait(false);
            var results = await slack.UpdateAsync(activity.Timestamp.ToString(), activity.ChannelId, message.text, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (!results.ok)
            {
                throw new Exception($"Error updating activity on Slack:{results}");
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
                SlackClientWrapper slack = await GetAPIAsync(turnContext.Activity).ConfigureAwait(false);
                var results = await slack.DeleteMessageAsync(reference.ChannelId, turnContext.Activity.Timestamp.Value.DateTime, cancellationToken).ConfigureAwait(false);
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
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ContinueConversationAsync(ConversationReference reference, BotCallbackHandler logic, CancellationToken cancellationToken)
        {
            var request = reference.GetContinuationActivity().ApplyConversationReference(reference, true); // TODO: check on this

            using (var context = new TurnContext(this, request))
            {
                await RunPipelineAsync(context, logic, cancellationToken).ConfigureAwait(false);
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
            // Create an Activity based on the incoming message from Slack.
            // There are a few different types of event that Slack might send.
            string body;
            using (var sr = new StreamReader(request.Body))
            {
                body = sr.ReadToEnd();
            }

            dynamic slackEvent = JsonConvert.DeserializeObject(body);

            if (slackEvent.type == "url_verification")
            {
                response.StatusCode = (int)HttpStatusCode.OK;
                response.ContentType = "text/plain";
                string text = slackEvent.challenge.ToString();
                await response.WriteAsync(text, cancellationToken).ConfigureAwait(false);
                return;
            }

            if (!SlackHelper.VerifySignature(_options.ClientSigningSecret, request, body))
            {
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
            else
            {
                if (slackEvent.payload != null)
                {
                    // handle interactive_message callbacks and block_actions
                    slackEvent = JsonConvert.ToString(slackEvent.payload);
                    if (!string.IsNullOrWhiteSpace(_options.VerificationToken) && slackEvent.token != _options.VerificationToken)
                    {
                        response.StatusCode = (int)HttpStatusCode.Forbidden;
                        response.ContentType = "text/plain";
                        string text = $"Rejected due to mismatched verificationToken:{slackEvent}";
                        await response.WriteAsync(text, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        Activity activity = new Activity()
                        {
                            Timestamp = default(DateTime),
                            ChannelId = "slack",
                            Conversation = new ConversationAccount()
                            {
                                Id = slackEvent.channel.id,
                            },
                            From = new ChannelAccount()
                            {
                                Id = (slackEvent.bot_id != null) ? slackEvent.bot_id : slackEvent.user.id,
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
                        activity.Conversation.Properties["team"] = slackEvent.team.id;

                        // this complains because of extra fields in conversation
                        activity.Recipient.Id = await GetBotUserByTeamAsync(activity).ConfigureAwait(false);

                        // create a conversation reference
                        using (var context = new TurnContext(this, activity))
                        {
                            context.TurnState.Add("httpStatus", ((int)HttpStatusCode.OK).ToString(System.Globalization.CultureInfo.InvariantCulture));

                            await RunPipelineAsync(context, bot.OnTurnAsync, cancellationToken).ConfigureAwait(false);

                            // send http response back
                            response.StatusCode = Convert.ToInt32(context.TurnState.Get<string>("httpStatus"), System.Globalization.CultureInfo.InvariantCulture);
                            response.ContentType = "text/plain";
                            string text = (context.TurnState.Get<object>("httpBody") != null) ? context.TurnState.Get<object>("httpBody").ToString() : string.Empty;
                            await response.WriteAsync(text, cancellationToken).ConfigureAwait(false);
                        }
                    }
                }
                else if (slackEvent.type == "event_callback")
                {
                    // this is an event api post
                    if (!string.IsNullOrWhiteSpace(_options.VerificationToken) && slackEvent.token != _options.VerificationToken)
                    {
                        response.StatusCode = (int)HttpStatusCode.Forbidden;
                        response.ContentType = "text/plain";
                        string text = $"Rejected due to mismatched verificationToken:{slackEvent}";
                        await response.WriteAsync(text, cancellationToken).ConfigureAwait(false);
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
                            ChannelData = SlackHelper.GetChannelDataFromSlackEvent(slackEvent),
                            Text = null,
                            Type = ActivityTypes.Event,
                        };

                        // Extra field that doesn't belong to activity
                        activity.Conversation.Properties["thread_ts"] = slackEvent["event"].thread_ts;

                        // this complains because of extra fields in conversation
                        activity.Recipient.Id = await GetBotUserByTeamAsync(activity).ConfigureAwait(false);

                        // Normalize the location of the team id
                        activity.GetChannelData<NewSlackMessage>().team = slackEvent.team_id;

                        // add the team id to the conversation record
                        activity.Conversation.Properties["team"] = activity.GetChannelData<NewSlackMessage>().team;

                        // If this is conclusively a message originating from a user, we'll mark it as such
                        if (slackEvent["event"].type == "message" && slackEvent["event"].subtype == null)
                        {
                            activity.Type = ActivityTypes.Message;
                            activity.Text = slackEvent["event"].text;
                        }

                        // create a conversation reference
                        using (var context = new TurnContext(this, activity))
                        {
                            context.TurnState.Add("httpStatus", ((int)HttpStatusCode.OK).ToString(System.Globalization.CultureInfo.InvariantCulture));

                            await RunPipelineAsync(context, bot.OnTurnAsync, cancellationToken).ConfigureAwait(false);

                            // send http response back
                            response.StatusCode = Convert.ToInt32(context.TurnState.Get<string>("httpStatus"), System.Globalization.CultureInfo.InvariantCulture);
                            response.ContentType = "text/plain";
                            string text = (context.TurnState.Get<object>("httpBody") != null) ? context.TurnState.Get<object>("httpBody").ToString() : string.Empty;
                            await response.WriteAsync(text, cancellationToken).ConfigureAwait(false);
                        }
                    }
                }
                else if (slackEvent.Command != null)
                {
                    if (!string.IsNullOrWhiteSpace(_options.VerificationToken) && slackEvent.Token != _options.VerificationToken)
                    {
                        response.StatusCode = (int)HttpStatusCode.Forbidden;
                        response.ContentType = "text/plain";
                        string text = $"Rejected due to mismatched verificationToken:{slackEvent}";
                        await response.WriteAsync(text, cancellationToken).ConfigureAwait(false);
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
                            ChannelData = SlackHelper.GetChannelDataFromSlackEvent(slackEvent),
                            Text = slackEvent.text,
                            Type = ActivityTypes.Event,
                        };

                        activity.Recipient.Id = await GetBotUserByTeamAsync(activity).ConfigureAwait(false);

                        // Normalize the location of the team id
                        activity.GetChannelData<NewSlackMessage>().team = slackEvent.TeamId;

                        // add the team id to the conversation record
                        activity.Conversation.Properties["team"] = activity.GetChannelData<NewSlackMessage>().team;

                        // create a conversation reference
                        using (var context = new TurnContext(this, activity))
                        {
                            context.TurnState.Add("httpStatus", ((int)HttpStatusCode.OK).ToString(System.Globalization.CultureInfo.InvariantCulture));

                            await RunPipelineAsync(context, bot.OnTurnAsync, cancellationToken).ConfigureAwait(false);

                            // send http response back
                            response.StatusCode = Convert.ToInt32(context.TurnState.Get<string>("httpStatus"), System.Globalization.CultureInfo.InvariantCulture);
                            response.ContentType = "text/plain";
                            string text = (context.TurnState.Get<object>("httpBody") != null) ? context.TurnState.Get<object>("httpBody").ToString() : string.Empty;
                            await response.WriteAsync(text).ConfigureAwait(false);
                        }
                    }
                }
                else
                {
                    throw new Exception($"Unknown Slack event type {slackEvent}");
                }
            }
        }

        private async Task LoginWithSlack()
        {
            if (_options.BotToken != null)
            {
                _identity = await _slackClient.TestAuthAsync(default).ConfigureAwait(false);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(_options.ClientId) || string.IsNullOrWhiteSpace(_options.ClientSecret) ||
                _options.RedirectUri != null || _options.GetScopes().Length > 0)
                {
                    throw new Exception("Missing Slack API credentials! Provide clientId, clientSecret, scopes and redirectUri as part of the SlackAdapter options.");
                }
            }
        }
    }
}
