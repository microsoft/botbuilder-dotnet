// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Schema;
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
        /// <param name="options">An object containing API credentials, a webhook verification token and other options.</param>
        public WebexAdapter(IWebexAdapterOptions options)
            : base()
        {
        }

        /// <summary>
        /// Gets a customized BotWorker.
        /// </summary>
        /// <value>A customized BotWorker object that exposes additional utility methods.</value>
        public WebexBotWorker BotkitWorker { get; private set; }

        /// <summary>
        /// Gets the identity of the bot.
        /// </summary>
        private string Identity { get; }

        /// <summary>
        /// Load the bot's identity via the Webex API.
        /// MUST be called by BotBuilder bots in order to filter messages sent by the bot.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<object> GetIdentity()
        {
            return await Task.FromException<object>(new NotImplementedException());
        }

        /// <summary>
        /// Botkit-only: Initialization function called automatically when used with Botkit.
        /// </summary>
        /// <param name="botkit">A botkit object.</param>
        public void Init(object botkit)
        {
        }

        /// <summary>
        /// Clears out and resets all the webhook subscriptions currently associated with this application.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<object> ResetWebhookSubscriptions()
        {
            return await Task.FromException<object>(new NotImplementedException());
        }

        /// <summary>
        /// Register a webhook subscription with Webex Teams to start receiving message events.
        /// </summary>
        /// <param name="webhookPath">The path of the webhook endpoint like `/api/messages`.</param>
        public void RegisterWebhookSubscription(string webhookPath)
        {
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
            return await Task.FromException<ResourceResponse[]>(new NotImplementedException());
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
            return await Task.FromException<ResourceResponse>(new NotImplementedException());
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
            await Task.FromException<ResourceResponse>(new NotImplementedException());
        }

        /// <summary>
        /// Standard BotBuilder adapter method for continuing an existing conversation based on a conversation reference.
        /// </summary>
        /// <param name="reference">A conversation reference to be applied to future messages.</param>
        /// <param name="logic">A bot logic function that will perform continuing action in the form 'async(context) => { ... }'.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ContinueConversationAsync(ConversationReference reference, BotCallbackHandler logic)
        {
            await Task.FromException(new NotImplementedException());
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
            await Task.FromException(new NotImplementedException());
        }
    }
}
