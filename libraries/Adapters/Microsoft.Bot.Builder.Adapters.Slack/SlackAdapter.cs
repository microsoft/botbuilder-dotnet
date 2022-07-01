﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Authentication;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Adapters.Slack.Model;
using Microsoft.Bot.Builder.Adapters.Slack.Model.Events;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Adapters.Slack
{
    [Obsolete("The Bot Framework Adapters will be deprecated in the next version of the Bot Framework SDK and moved to https://github.com/BotBuilderCommunity/botbuilder-community-dotnet. Please refer to their new location for all future work.")]
    public class SlackAdapter : BotAdapter, IBotFrameworkHttpAdapter
    {
        private const string SlackVerificationTokenKey = "SlackVerificationToken";
        private const string SlackBotTokenKey = "SlackBotToken";
        private const string SlackClientSigningSecretKey = "SlackClientSigningSecret";

        private readonly SlackClientWrapper _slackClient;
        private readonly ILogger _logger;
        private readonly SlackAdapterOptions _options;
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings { MaxDepth = null };

        /// <summary>
        /// Initializes a new instance of the <see cref="SlackAdapter"/> class using configuration settings.
        /// </summary>
        /// <param name="configuration">An <see cref="IConfiguration"/> instance.</param>
        /// <remarks>
        /// The configuration keys are:
        /// SlackVerificationToken: A token for validating the origin of incoming webhooks.
        /// SlackBotToken: A token for a bot to work on a single workspace.
        /// SlackClientSigningSecret: The token used to validate that incoming webhooks are originated from Slack.
        /// </remarks>
        /// <param name="options">An instance of <see cref="SlackAdapterOptions"/>.</param>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        public SlackAdapter(IConfiguration configuration, SlackAdapterOptions options = null, ILogger logger = null)
            : this(new SlackClientWrapper(new SlackClientWrapperOptions(configuration[SlackVerificationTokenKey], configuration[SlackBotTokenKey], configuration[SlackClientSigningSecretKey])), options, logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SlackAdapter"/> class.
        /// Creates a Slack adapter.
        /// </summary>
        /// <param name="adapterOptions">The adapter options to be used when connecting to the Slack API.</param>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        /// <param name="slackClient">The SlackClientWrapper used to connect to the Slack API.</param>
        public SlackAdapter(SlackClientWrapper slackClient, SlackAdapterOptions adapterOptions, ILogger logger = null)
        {
            _slackClient = slackClient ?? throw new ArgumentNullException(nameof(adapterOptions));
            _logger = logger ?? NullLogger.Instance;
            _options = adapterOptions ?? new SlackAdapterOptions();
        }

        /// <summary>
        /// Standard BotBuilder adapter method to send a message from the bot to the messaging API.
        /// </summary>
        /// <param name="turnContext">A TurnContext representing the current incoming message and environment.</param>
        /// <param name="activities">An array of outgoing activities to be sent back to the messaging API.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>An array of <see cref="ResourceResponse"/> objects containing the IDs that Slack assigned to the sent messages.</returns>
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

            foreach (var activity in activities)
            {
                if (activity.Type != ActivityTypes.Message)
                {
                    _logger.LogTrace($"Unsupported Activity Type: '{activity.Type}'. Only Activities of type 'Message' are supported.");
                }
                else
                {
                    var message = SlackHelper.ActivityToSlack(activity);

                    var slackResponse = await _slackClient.PostMessageAsync(message, cancellationToken)
                        .ConfigureAwait(false);

                    if (slackResponse != null && slackResponse.Ok)
                    {
                        var resourceResponse = new ActivityResourceResponse()
                        {
                            Id = slackResponse.Ts,
                            ActivityId = slackResponse.Ts,
                            Conversation = new ConversationAccount() { Id = slackResponse.Channel, },
                        };
                        responses.Add(resourceResponse);
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

            var results = await _slackClient.UpdateAsync(message.Ts, message.Channel, message.Text, cancellationToken: cancellationToken).ConfigureAwait(false);

            if (!results.Ok)
            {
                throw new InvalidOperationException($"Error updating activity on Slack:{results}");
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

            await _slackClient.DeleteMessageAsync(reference.ChannelId, turnContext.Activity.Timestamp.Value.DateTime, cancellationToken).ConfigureAwait(false);
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
        /// Accept an incoming webhook request and convert it into a TurnContext which can be processed by the bot's logic.
        /// </summary>
        /// <param name="request">The incoming HTTP request.</param>
        /// <param name="response">When this method completes, the HTTP response to send.</param>
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

            string body;

            Activity activity = null;

            using (var sr = new StreamReader(request.Body))
            {
                body = await sr.ReadToEndAsync().ConfigureAwait(false);
            }

            if (_options.VerifyIncomingRequests && !_slackClient.VerifySignature(request, body))
            {
                const string text = "Rejected due to mismatched header signature";
                await SlackHelper.WriteAsync(response, HttpStatusCode.Unauthorized, text, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
                throw new AuthenticationException(text);
            }

            var requestContentType = request.Headers["Content-Type"].ToString();

            if (requestContentType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
            {
                var postValues = SlackHelper.QueryStringToDictionary(body);

                if (postValues.ContainsKey("payload"))
                {
                    var payload = JsonConvert.DeserializeObject<InteractionPayload>(postValues["payload"], _settings);
                    activity = SlackHelper.PayloadToActivity(payload);
                }
                else if (postValues.ContainsKey("command"))
                {
                    var serializedPayload = JsonConvert.SerializeObject(postValues, _settings);
                    var payload = JsonConvert.DeserializeObject<CommandPayload>(serializedPayload, _settings);
                    activity = SlackHelper.CommandToActivity(payload, _slackClient);
                }
            }
            else if (requestContentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase))
            {
                var bodyObject = JObject.Parse(body);

                if (bodyObject["type"]?.ToString() == "url_verification")
                {
                    var verificationEvent = bodyObject.ToObject<UrlVerificationEvent>();
                    await SlackHelper.WriteAsync(response, HttpStatusCode.OK, verificationEvent.Challenge, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
                    return;
                }

                if (!string.IsNullOrWhiteSpace(_slackClient.Options.SlackVerificationToken) && bodyObject["token"]?.ToString() != _slackClient.Options.SlackVerificationToken)
                {
                    var text = $"Rejected due to mismatched verificationToken:{bodyObject["token"]}";
                    await SlackHelper.WriteAsync(response, HttpStatusCode.Forbidden, text, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
                    throw new AuthenticationException(text);
                }

                if (bodyObject["type"]?.ToString() == "event_callback")
                {
                    // this is an event api post
                    var eventRequest = bodyObject.ToObject<EventRequest>();
                    activity = SlackHelper.EventToActivity(eventRequest, _slackClient);
                }
            }

            // As per official Slack API docs, some additional request types may be receieved that can be ignored
            // but we should respond with a 200 status code
            // https://api.slack.com/interactivity/slash-commands
            if (activity == null)
            {
                await SlackHelper.WriteAsync(response, HttpStatusCode.OK, "Unable to transform request / payload into Activity. Possible unrecognized request type", Encoding.UTF8, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                using (var context = new TurnContext(this, activity))
                {
                    context.TurnState.Add("httpStatus", ((int)HttpStatusCode.OK).ToString(System.Globalization.CultureInfo.InvariantCulture));

                    await RunPipelineAsync(context, bot.OnTurnAsync, cancellationToken).ConfigureAwait(false);

                    var code = Convert.ToInt32(context.TurnState.Get<string>("httpStatus"), System.Globalization.CultureInfo.InvariantCulture);
                    var statusCode = (HttpStatusCode)code;
                    var text = context.TurnState.Get<object>("httpBody") != null ? context.TurnState.Get<object>("httpBody").ToString() : string.Empty;

                    await SlackHelper.WriteAsync(response, statusCode, text, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Determines whether the provided <see cref="IConfiguration"/> has the settings needed to
        /// configure a <see cref="SlackAdapter"/>.
        /// </summary>
        /// <param name="configuration"><see cref="IConfiguration"/> to verify for settings.</param>
        /// <returns>A value indicating whether the configuration has the necessary settings required to create a <see cref="SlackAdapter"/>.</returns>
        internal static bool HasConfiguration(IConfiguration configuration)
        {
            // Do we have the config needed to create an adapter?
            return !string.IsNullOrEmpty(configuration.GetValue<string>(SlackBotTokenKey))
                && !string.IsNullOrEmpty(configuration.GetValue<string>(SlackClientSigningSecretKey))
                && !string.IsNullOrEmpty(configuration.GetValue<string>(SlackVerificationTokenKey));
        }
    }
}
