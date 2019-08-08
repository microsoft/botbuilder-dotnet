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
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Microsoft.Bot.Builder.Adapters.Twilio
{
    public class TwilioAdapter : BotAdapter
    {
        private readonly ITwilioAdapterOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="TwilioAdapter"/> class.
        /// A Twilio adapter will allow the Bot to connect to Twilio's SMS service.
        /// </summary>
        /// <param name="options">A set of params with the required values for authentication.</param>
        public TwilioAdapter(ITwilioAdapterOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (string.IsNullOrWhiteSpace(options.TwilioNumber))
            {
                throw new Exception("TwilioNumber is a required part of the configuration.");
            }

            if (string.IsNullOrWhiteSpace(options.AccountSid))
            {
                throw new Exception("AccountSid is a required part of the configuration.");
            }

            if (string.IsNullOrWhiteSpace(options.AuthToken))
            {
                throw new Exception("AuthToken is a required part of the configuration.");
            }

            _options = options;

            TwilioClient.Init(_options.AccountSid, _options.AuthToken);
        }

        /// <summary>
        /// Standard BotBuilder adapter method to send a message from the bot to the messaging API.
        /// </summary>
        /// <param name="turnContext">A TurnContext representing the current incoming message and environment.</param>
        /// <param name="activities">An array of outgoing activities to be sent back to the messaging API.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A resource response array with the message Sids.</returns>
        public override async Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)
        {
            var responses = new List<ResourceResponse>();
            foreach (var activity in activities)
            {
                if (activity.Type == ActivityTypes.Message)
                {
                    var messageOptions = TwilioHelper.ActivityToTwilio(activity, _options.TwilioNumber);

                    var res = await MessageResource.CreateAsync(messageOptions).ConfigureAwait(false);

                    var response = new ResourceResponse()
                    {
                        Id = res.Sid,
                    };

                    responses.Add(response);
                }
                else
                {
                    throw new Exception("Unknown message type");
                }
            }

            return responses.ToArray();
        }

        /// <summary>
        /// Accept an incoming webhook httpRequest and convert it into a TurnContext which can be processed by the bot's logic.
        /// </summary>
        /// <param name="httpRequest">A httpRequest object.</param>
        /// <param name="httpResponse">A httpResponse object.</param>
        /// <param name="bot">A bot with logic function in the form `async(context) => { ... }`.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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
        /// Standard BotBuilder adapter method to update a previous message with new content.
        /// </summary>
        /// <param name="turnContext">A TurnContext representing the current incoming message and environment.</param>
        /// <param name="activity">The updated activity in the form '{id: `id of activity to update`, ...}'.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A resource response with the Id of the updated activity.</returns>
        public override Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            // Twilio adapter does not support updateActivity.
            return Task.FromException<ResourceResponse>(new NotSupportedException("Twilio SMS does not support updating activities."));
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
            // Twilio adapter does not support deleteActivity.
            return Task.FromException<ResourceResponse>(new NotSupportedException("Twilio SMS does not support deleting activities."));
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
    }
}
