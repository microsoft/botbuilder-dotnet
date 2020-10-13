// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents;
using Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents.Handover;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Facebook
{
     /// <summary>
     /// BotAdapter to allow for handling Facebook App payloads and responses via the Facebook API.
     /// </summary>
    public class FacebookAdapter : BotAdapter, IBotFrameworkHttpAdapter
    {
        private const string HubModeSubscribe = "subscribe";

        /// <summary>
        /// An instance of the FacebookClientWrapper class.
        /// </summary>
        private readonly FacebookClientWrapper _facebookClient;
        private readonly ILogger _logger;
        private readonly FacebookAdapterOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="FacebookAdapter"/> class using configuration settings.
        /// </summary>
        /// <param name="configuration">An <see cref="IConfiguration"/> instance.</param>
        /// <remarks>
        /// The adapter uses these configuration keys:
        /// - `VerifyToken`, the token to respond to the initial verification request.
        /// - `AppSecret`, the secret used to validate incoming webhooks.
        /// - `AccessToken`, an access token for the bot.
        /// </remarks>
        /// <param name="options">An instance of <see cref="FacebookAdapterOptions"/>.</param>
        /// <param name="logger">The logger this adapter should use.</param>
        public FacebookAdapter(IConfiguration configuration, FacebookAdapterOptions options = null, ILogger logger = null)
            : this(new FacebookClientWrapper(new FacebookClientWrapperOptions(configuration["FacebookVerifyToken"], configuration["FacebookAppSecret"], configuration["FacebookAccessToken"])), options, logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FacebookAdapter"/> class using an existing Facebook client.
        /// </summary>
        /// /// <param name="facebookClient">Client used to interact with the Facebook API.</param>
        /// <param name="options">Options for the Facebook Adapter.</param>
        /// <param name="logger">The logger this adapter should use.</param>
        /// <exception cref="ArgumentNullException"><paramref name="options"/> is null.</exception>
        public FacebookAdapter(FacebookClientWrapper facebookClient, FacebookAdapterOptions options, ILogger logger = null)
        {
            _options = options ?? new FacebookAdapterOptions();
            _facebookClient = facebookClient ?? throw new ArgumentNullException(nameof(facebookClient));
            _logger = logger ?? NullLogger.Instance;
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
        /// an array of <see cref="ResourceResponse"/> objects containing the IDs that
        /// the receiving channel assigned to the activities.</remarks>
        public override async Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)
        {
            var responses = new List<ResourceResponse>();

            foreach (var activity in activities)
            {
                if (activity.Type != ActivityTypes.Message && activity.Type != ActivityTypes.Event)
                {
                    _logger.LogTrace($"Unsupported Activity Type: '{activity.Type}'. Only Activities of type 'Message' or 'Event' are supported.");
                }
                else
                {
                    var message = CreateFacebookMessageFromActivity(activity);

                    if (message.Message?.Attachment != null)
                    {
                        message.Message.Text = null;
                    }

                    var res = await _facebookClient.SendMessageAsync("/me/messages", message, null, cancellationToken)
                        .ConfigureAwait(false);

                    if (activity.Type == ActivityTypes.Event)
                    {
                        if (activity.Name.Equals(HandoverConstants.PassThreadControl, StringComparison.Ordinal))
                        {
                            var recipient = (string)activity.Value == "inbox" ? HandoverConstants.PageInboxId : (string)activity.Value;
                            await _facebookClient.PassThreadControlAsync(recipient, activity.Conversation.Id, HandoverConstants.MetadataPassThreadControl, cancellationToken).ConfigureAwait(false);
                        }
                        else if (activity.Name.Equals(HandoverConstants.TakeThreadControl, StringComparison.Ordinal))
                        {
                            await _facebookClient.TakeThreadControlAsync(activity.Conversation.Id, HandoverConstants.MetadataTakeThreadControl, cancellationToken).ConfigureAwait(false);
                        }
                        else if (activity.Name.Equals(HandoverConstants.RequestThreadControl, StringComparison.Ordinal))
                        {
                            await _facebookClient.RequestThreadControlAsync(activity.Conversation.Id, HandoverConstants.MetadataRequestThreadControl, cancellationToken).ConfigureAwait(false);
                        }
                    }

                    var response = new ResourceResponse()
                    {
                        Id = res,
                    };

                    responses.Add(response);
                }
            }

            return responses.ToArray();
        }

        /// <summary>
        /// Throws a <see cref="NotImplementedException"/> exception in all cases.
        /// </summary>
        /// <param name="turnContext">The context object for the turn.</param>
        /// <param name="activity">New replacement activity.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public override Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            return Task.FromException<ResourceResponse>(new NotImplementedException("Facebook adapter does not support updateActivity."));
        }

        /// <summary>
        /// Throws a <see cref="NotImplementedException"/> exception in all cases.
        /// </summary>
        /// <param name="turnContext">The context object for the turn.</param>
        /// <param name="reference">Conversation reference for the activity to delete.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public override Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken)
        {
            return Task.FromException(new NotImplementedException("Facebook adapter does not support deleteActivity."));
        }

        /// <summary>
        /// Sends a proactive message to a conversation using a conversation reference.
        /// </summary>
        /// <param name="reference">A reference to the conversation to continue.</param>
        /// <param name="logic">The method to call for the resulting bot turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>Call this method to proactively send a message to a conversation.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="logic"/> or
        /// <paramref name="reference"/> is null.</exception>
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
        /// Accepts an incoming webhook request, creates a turn context,
        /// and runs the middleware pipeline for an incoming TRUSTED activity.
        /// </summary>
        /// <param name="httpRequest">Represents the incoming side of an HTTP request.</param>
        /// <param name="httpResponse">Represents the outgoing side of an HTTP request.</param>
        /// <param name="bot">The code to run at the end of the adapter's middleware pipeline.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <exception cref="AuthenticationException">The webhook receives message with invalid signature.</exception>
        public async Task ProcessAsync(HttpRequest httpRequest, HttpResponse httpResponse, IBot bot, CancellationToken cancellationToken = default)
        {
            if (httpRequest.Query["hub.mode"] == HubModeSubscribe && _options.VerifyIncomingRequests)
            {
                await _facebookClient.VerifyWebhookAsync(httpRequest, httpResponse, cancellationToken).ConfigureAwait(false);
                return;
            }

            string stringifiedBody;

            using (var sr = new StreamReader(httpRequest.Body))
            {
                stringifiedBody = await sr.ReadToEndAsync().ConfigureAwait(false);
            }

            if (!_facebookClient.VerifySignature(httpRequest, stringifiedBody) && _options.VerifyIncomingRequests)
            {
                await FacebookHelper.WriteAsync(httpResponse, HttpStatusCode.Unauthorized, string.Empty, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
                throw new AuthenticationException("Webhook received message with invalid signature. Potential malicious behavior!");
            }

            FacebookResponseEvent facebookResponseEvent = null;

            facebookResponseEvent = JsonConvert.DeserializeObject<FacebookResponseEvent>(stringifiedBody);

            foreach (var entry in facebookResponseEvent.Entry)
            {
                var payload = entry.Changes.Count > 0 ? entry.Changes : entry.Messaging.Count > 0 ? entry.Messaging : entry.Standby.Count > 0 ? entry.Standby : new List<FacebookMessage>();

                foreach (var message in payload)
                {
                    message.IsStandby = entry.Standby.Count > 0;

                    var activity = FacebookHelper.ProcessSingleMessage(message);

                    using (var context = new TurnContext(this, activity))
                    {
                        await RunPipelineAsync(context, bot.OnTurnAsync, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
        }

        /// <summary>
        /// Factory method to create the <see cref="FacebookMessage"/> instance of the <see cref="Activity"/> to be sent to Facebook.
        /// </summary>
        /// <remarks>
        /// This lets an override add a Facebook-supported message tag to an outgoing message.
        /// See https://developers.facebook.com/docs/messenger-platform/send-messages/message-tags/.
        /// </remarks>
        /// <param name="activity">An <see cref="Activity"/> instance to build the message.</param>
        /// <returns>A <see cref="FacebookMessage"/> built from the activity instance.</returns>
        protected virtual FacebookMessage CreateFacebookMessageFromActivity(Activity activity)
        {
            return FacebookHelper.ActivityToFacebook(activity);
        }
    }
}
