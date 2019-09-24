// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Adapters.Slack
{
    public class SlackAdapter : BotAdapter, IBotFrameworkHttpAdapter
    {
        private const string SlackOAuthUrl = "https://slack.com/oauth/authorize?client_id=";

        private readonly SlackClientWrapper _slackClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlackAdapter"/> class.
        /// Create a Slack adapter.
        /// </summary>
        /// <param name="slackClient">An initialized instance of the SlackClientWrapper class.</param>
        public SlackAdapter(SlackClientWrapper slackClient)
            : base()
        {
            _slackClient = slackClient ?? throw new ArgumentNullException(nameof(slackClient));

            _slackClient.LoginWithSlackAsync(default).Wait();
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
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (activities == null)
            {
                throw new ArgumentNullException(nameof(activities));
            }

            var responses = new List<ResourceResponse>();

            for (var i = 0; i < activities.Length; i++)
            {
                var activity = activities[i];

                if (activity.Type == ActivityTypes.Message)
                {
                    var message = SlackHelper.ActivityToSlack(activity);

                    var slackResponse = await _slackClient.PostMessageAsync(message, cancellationToken).ConfigureAwait(false);

                    if (slackResponse != null && slackResponse.Ok)
                    {
                        var resourceResponse = new ActivityResourceResponse()
                        {
                            Id = slackResponse.TS,
                            ActivityId = slackResponse.TS,
                            Conversation = new ConversationAccount()
                            {
                                Id = slackResponse.Channel,
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
                throw new ArgumentException(nameof(activity.Timestamp));
            }

            if (activity.Conversation == null)
            {
                throw new ArgumentException(nameof(activity.ChannelId));
            }

            var message = SlackHelper.ActivityToSlack(activity);
            var results = await _slackClient.UpdateAsync(activity.Timestamp.ToString(), activity.ChannelId, message.text, cancellationToken: cancellationToken).ConfigureAwait(false);
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
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (reference == null)
            {
                throw new ArgumentNullException(nameof(reference));
            }

            if (reference.ChannelId == null)
            {
                throw new ArgumentException(nameof(reference.ChannelId));
            }

            if (turnContext.Activity.Timestamp == null)
            {
                throw new ArgumentException(nameof(turnContext.Activity.Timestamp));
            }

            var results = await _slackClient.DeleteMessageAsync(reference.ChannelId, turnContext.Activity.Timestamp.Value.DateTime, cancellationToken).ConfigureAwait(false);
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
            if (reference == null)
            {
                throw new ArgumentNullException(nameof(reference));
            }

            if (logic == null)
            {
                throw new ArgumentNullException(nameof(logic));
            }

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

            string body;

            using (var sr = new StreamReader(request.Body))
            {
                body = sr.ReadToEnd();
            }

            var slackBody = SlackHelper.DeserializeBody(body);

            if (slackBody.Type == "url_verification")
            {
                var text = slackBody.Challenge;

                await SlackHelper.WriteAsync(response, HttpStatusCode.OK, text, Encoding.UTF8, cancellationToken).ConfigureAwait(false);

                return;
            }

            if (!_slackClient.VerifySignature(request, body))
            {
                var text = $"Rejected due to mismatched header signature";

                await SlackHelper.WriteAsync(response, HttpStatusCode.Unauthorized, text, Encoding.UTF8, cancellationToken).ConfigureAwait(false);

                throw new Exception(text);
            }

            if (!string.IsNullOrWhiteSpace(_slackClient.Options.VerificationToken) && slackBody.Token != _slackClient.Options.VerificationToken)
            {
                var text = $"Rejected due to mismatched verificationToken:{slackBody}";

                await SlackHelper.WriteAsync(response, HttpStatusCode.Forbidden, text, Encoding.UTF8, cancellationToken).ConfigureAwait(false);

                throw new Exception(text);
            }

            var activity = new Activity();

            if (slackBody.Payload != null)
            {
                // handle interactive_message callbacks and block_actions
                activity = SlackHelper.PayloadToActivity(slackBody.Payload);
            }
            else if (slackBody.Type == "event_callback")
            {
                // this is an event api post
                activity = await SlackHelper.EventToActivityAsync(slackBody.Event, _slackClient, cancellationToken).ConfigureAwait(false);
            }
            else if (slackBody.Command != null)
            {
                activity = await SlackHelper.CommandToActivityAsync(slackBody, _slackClient, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                throw new Exception($"Unknown Slack event type {slackBody.Type}");
            }

            using (var context = new TurnContext(this, activity))
            {
                context.TurnState.Add("httpStatus", ((int)HttpStatusCode.OK).ToString(System.Globalization.CultureInfo.InvariantCulture));

                await RunPipelineAsync(context, bot.OnTurnAsync, cancellationToken).ConfigureAwait(false);

                // send http response back
                var statusCode = Convert.ToInt32(context.TurnState.Get<string>("httpStatus"), System.Globalization.CultureInfo.InvariantCulture);
                var text = (context.TurnState.Get<object>("httpBody") != null) ? context.TurnState.Get<object>("httpBody").ToString() : string.Empty;

                await SlackHelper.WriteAsync(response, HttpStatusCode.OK, text, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
