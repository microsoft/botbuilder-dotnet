// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Security;

namespace Microsoft.Bot.Builder.Adapters.Twilio
{
    public class TwilioAdapter : BotAdapter
    {
        public const string NAME = "Twilio SMS Adapter";

        private readonly ITwilioAdapterOptions options;

        public TwilioAdapter(ITwilioAdapterOptions options)
        {
            this.options = options;

            if (options.TwilioNumber.Equals(string.Empty))
            {
                throw new Exception("TwilioNumber is a required part of the configuration.");
            }

            if (options.AccountSID.Equals(string.Empty))
            {
                throw new Exception("AccountSID is a required part of the configuration.");
            }

            if (options.AuthToken.Equals(string.Empty))
            {
                throw new Exception("AuthToken is a required part of the configuration.");
            }

            TwilioClient.Init(this.options.AccountSID, this.options.AuthToken);
        }

        public TwilioBotWorker BotkitWorker { get; private set; }

        /// <summary>
        /// Standard BotBuilder adapter method to send a message from the bot to the messaging API.
        /// </summary>
        /// <param name="turnContext">A TurnContext representing the current incoming message and environment.</param>
        /// <param name="activities">An array of outgoing activities to be sent back to the messaging API.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A resource response.</returns>
        public override async Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)
        {
            List<ResourceResponse> responses = new List<ResourceResponse>();
            for (var i = 0; i < activities.Length; i++)
            {
                var activity = activities[i];
                if (activity.Type == ActivityTypes.Message)
                {
                    var messageOptions = this.ActivityToTwilio(activity);

                    var res = await MessageResource.CreateAsync(messageOptions);

                    var response = new ResourceResponse()
                    {
                        Id = res.Sid,
                    };

                    responses.Add(response);
                }
                else
                {
                    // log error: unknown message type
                }
            }

            return responses.ToArray();
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

            var twilioSignature = request.Headers["x-twilio-signature"];

            var validationURL = this.options.ValidationURL ?? (request.Headers["x-forwarded-proto"][0] ?? request.Protocol + "://" + request.Host + request.Path);

            var requestValidator = new RequestValidator(this.options.AuthToken);

            var bodyStream = new StreamReader(request.Body);

            dynamic payload = bodyStream.ReadToEnd();

            var formatedPayload = payload.Replace("+", "%20");

            var body = QueryStringToDictionary(formatedPayload);

            if (!twilioSignature.Equals(string.Empty) && requestValidator.Validate(validationURL, body, twilioSignature))
            {
                var jsonBody = JsonConvert.SerializeObject(body);

                TwilioEvent twilioEvent = JsonConvert.DeserializeObject<TwilioEvent>(jsonBody);

                var activity = new Activity()
                {
                    Id = twilioEvent.MessageSid,
                    Timestamp = new DateTime(),
                    ChannelId = "twilio-sms",
                    Conversation = new ConversationAccount()
                    {
                        Id = twilioEvent.From.ToString(),
                    },
                    From = new ChannelAccount()
                    {
                        Id = twilioEvent.From.ToString(),
                    },
                    Recipient = new ChannelAccount()
                    {
                        Id = twilioEvent.To.ToString(),
                    },
                    Text = (twilioEvent as dynamic).Body,
                    ChannelData = twilioEvent,
                    Type = ActivityTypes.Message,
                };

                // Detect attachments
                if (Convert.ToInt32(twilioEvent.NumMedia) > 0)
                {
                    // specify a different event type for Botkit
                    (activity.ChannelData as dynamic).botkitEventType = "picture_message";
                }

                // create a conversation reference
                using (var context = new TurnContext(this, activity))
                {
                    await this.RunPipelineAsync(context, bot.OnTurnAsync, default(CancellationToken));

                    response.StatusCode = 200;
                    response.ContentType = "text/plain";
                    string text = (context.TurnState["httpBody"] != null) ? context.TurnState["httpBody"].ToString() : string.Empty;
                    await response.WriteAsync(text);
                }
            }
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
            // Twilio adapter does not support updateActivity.
            return await Task.FromException<ResourceResponse>(new NotImplementedException("Twilio SMS does not support updating activities."));
        }

        /// <summary>
        /// Standard BotBuilder adapter method to delete a previous message.
        /// </summary>
        /// <param name="turnContext">A TurnContext representing the current incoming message and environment.</param>
        /// <param name="reference">An object in the form "{activityId: `id of message to delete`, conversation: { id: `id of channel`}}".</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken)
        {
            // Twilio adapter does not support deleteActivity.
            await Task.FromException<ResourceResponse>(new NotImplementedException("Twilio SMS does not support deleting activities."));
        }

        /// <summary>
        /// Formats a BotBuilder activity into an outgoing Twilio SMS message.
        /// </summary>
        /// <param name="activity">A BotBuilder Activity object.</param>
        /// <returns>A Message's options object with {body, from, to, mediaUrl}.</returns>
        private CreateMessageOptions ActivityToTwilio(Activity activity)
        {
            var mediaURLs = new List<Uri>();

            if ((activity.ChannelData as dynamic)?.mediaURL != null)
            {
                mediaURLs.Add(new Uri((activity.ChannelData as dynamic).mediaURL));
            }

            var messageOptions = new CreateMessageOptions(activity.Conversation.Id);

            messageOptions.ApplicationSid = activity.Conversation.Id;
            messageOptions.From = this.options.TwilioNumber;
            messageOptions.Body = activity.Text;
            messageOptions.MediaUrl = mediaURLs;

            return messageOptions;
        }

        /// <summary>
        /// Standard BotBuilder adapter method for continuing an existing conversation based on a conversation reference.
        /// </summary>
        /// <param name="reference">A conversation reference to be applied to future messages.</param>
        /// <param name="logic">A bot logic function that will perform continuing action in the form 'async(context) => { ... }'.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task ContinueConversationAsync(ConversationReference reference, BotCallbackHandler logic)
        {
            var request = reference.GetContinuationActivity().ApplyConversationReference(reference, true);

            using (var context = new TurnContext(this, request))
            {
                await this.RunPipelineAsync(context, logic, default(CancellationToken));
            }
        }

        /// <summary>
        /// Converts a query string to a dictionary with key-value pairs.
        /// </summary>
        /// <param name="query">The query string to convert.</param>
        /// <returns>A dictionary with the query values.</returns>
        private Dictionary<string, string> QueryStringToDictionary(string query)
        {
            var pairs = query.Split('&');
            var values = new Dictionary<string, string>();

            foreach (var p in pairs)
            {
                var pair = p.Split('=');
                var key = pair[0];
                var value = Uri.UnescapeDataString(pair[1]);

                values.Add(key, value);
            }

            return values;
        }
    }
}
