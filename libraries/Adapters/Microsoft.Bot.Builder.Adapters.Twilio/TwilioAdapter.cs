// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Adapters.Twilio
{
    /// <summary>
    /// A <see cref="BotAdapter"/> that can connect to Twilio's SMS service.
    /// </summary>
    public class TwilioAdapter : BotAdapter
    {
        private readonly TwilioAdapterOptions _options;

        private readonly TwilioClientWrapper _twilioClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="TwilioAdapter"/> class.
        /// </summary>
        /// <param name="options">The options to use to authenticate the bot with the Twilio service.</param>
        /// <param name="twilioClient">The Twilio client to connect to.</param>
        /// <exception cref="ArgumentNullException"><paramref name="options"/> is <c>null</c>.</exception>
        public TwilioAdapter(TwilioAdapterOptions options, TwilioClientWrapper twilioClient)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (string.IsNullOrWhiteSpace(options.TwilioNumber))
            {
                throw new ArgumentException("TwilioNumber is a required part of the configuration.", nameof(options));
            }

            if (string.IsNullOrWhiteSpace(options.AccountSid))
            {
                throw new ArgumentException("AccountSid is a required part of the configuration.", nameof(options));
            }

            if (string.IsNullOrWhiteSpace(options.AuthToken))
            {
                throw new ArgumentException("AuthToken is a required part of the configuration.", nameof(options));
            }

            _twilioClient = twilioClient ?? throw new ArgumentNullException(nameof(twilioClient));
            _options = options;

            _twilioClient.LogIn(_options.AccountSid, _options.AuthToken);
        }

        /// <summary>
        /// Sends activities to the conversation.
        /// </summary>
        /// <param name="turnContext">The context object for the turn.</param>
        /// <param name="activities">The activities to send.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the activities are successfully sent, the task result contains
        /// an array of <see cref="ResourceResponse"/> objects containing the SIDs that
        /// Twilio assigned to the activities.</remarks>
        /// <seealso cref="ITurnContext.OnSendActivities(SendActivitiesHandler)"/>
        public override async Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)
        {
            var responses = new List<ResourceResponse>();
            foreach (var activity in activities)
            {
                if (activity.Type == ActivityTypes.Message)
                {
                    var messageOptions = TwilioHelper.ActivityToTwilio(activity, _options.TwilioNumber);

                    var res = await _twilioClient.SendMessage(messageOptions).ConfigureAwait(false);

                    var response = new ResourceResponse()
                    {
                        Id = res,
                    };

                    responses.Add(response);
                }
                else
                {
                    throw new ArgumentException("Unknown message type of Activity.", nameof(activities));
                }
            }

            return responses.ToArray();
        }

        /// <summary>
        /// Creates a turn context and runs the middleware pipeline for an incoming activity.
        /// </summary>
        /// <param name="httpRequest">The incoming HTTP request.</param>
        /// <param name="httpResponse">When this method completes, the HTTP response to send.</param>
        /// <param name="bot">The bot that will handle the incoming activity.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="httpRequest"/>,
        /// <paramref name="httpResponse"/>, or <paramref name="bot"/> is <c>null</c>.</exception>
        public async Task ProcessAsync(HttpRequest httpRequest, HttpResponse httpResponse, IBot bot, CancellationToken cancellationToken = default)
        {
            if (httpRequest == null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }

            if (httpResponse == null)
            {
                throw new ArgumentNullException(nameof(httpResponse));
            }

            if (bot == null)
            {
                throw new ArgumentNullException(nameof(bot));
            }

            var activity = TwilioHelper.RequestToActivity(httpRequest, _options.ValidationUrl, _options.AuthToken);

            // create a conversation reference
            using (var context = new TurnContext(this, activity))
            {
                context.TurnState.Add("httpStatus", HttpStatusCode.OK.ToString("D"));
                await RunPipelineAsync(context, bot.OnTurnAsync, cancellationToken).ConfigureAwait(false);

                httpResponse.StatusCode = Convert.ToInt32(context.TurnState.Get<string>("httpStatus"), CultureInfo.InvariantCulture);
                httpResponse.ContentType = "text/plain";
                var text = context.TurnState.Get<object>("httpBody") != null ? context.TurnState.Get<object>("httpBody").ToString() : string.Empty;

                await httpResponse.WriteAsync(text, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Replaces an existing activity in the conversation.
        /// Twilio SMS does not support this operation.
        /// </summary>
        /// <param name="turnContext">The context object for the turn.</param>
        /// <param name="activity">New replacement activity.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>This method always returns a faulted task.</remarks>
        /// <seealso cref="ITurnContext.OnUpdateActivity(UpdateActivityHandler)"/>
        public override Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            // Twilio adapter does not support updateActivity.
            return Task.FromException<ResourceResponse>(new NotSupportedException("Twilio SMS does not support updating activities."));
        }

        /// <summary>
        /// Deletes an existing activity in the conversation.
        /// Twilio SMS does not support this operation.
        /// </summary>
        /// <param name="turnContext">The context object for the turn.</param>
        /// <param name="reference">Conversation reference for the activity to delete.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>This method always returns a faulted task.</remarks>
        /// <seealso cref="ITurnContext.OnDeleteActivity(DeleteActivityHandler)"/>
        public override Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken)
        {
            // Twilio adapter does not support deleteActivity.
            return Task.FromException<ResourceResponse>(new NotSupportedException("Twilio SMS does not support deleting activities."));
        }

        /// <summary>
        /// Sends a proactive message to a conversation.
        /// </summary>
        /// <param name="reference">A reference to the conversation to continue.</param>
        /// <param name="logic">The method to call for the resulting bot turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>Call this method to proactively send a message to a conversation.
        /// Most channels require a user to initiate a conversation with a bot
        /// before the bot can send activities to the user.</remarks>
        /// <seealso cref="BotAdapter.RunPipelineAsync(ITurnContext, BotCallbackHandler, CancellationToken)"/>
        /// <exception cref="ArgumentNullException"><paramref name="reference"/> or
        /// <paramref name="logic"/> is <c>null</c>.</exception>
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
    }
}
