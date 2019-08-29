// Copyright (c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Thrzn41.WebexTeams;
using Thrzn41.WebexTeams.Version1;

namespace Microsoft.Bot.Builder.Adapters.Webex
{
    public class WebexApi : IWebexClient
    {
        private TeamsAPIClient _api;

        /// <summary>
        /// Creates a Webex client by supplying the access token.
        /// </summary>
        /// <param name="accessToken">The access token of the Webex account.</param>
        public void CreateClient(string accessToken)
        {
            _api = TeamsAPI.CreateVersion1Client(accessToken);
        }

        /// <summary>
        /// Wraps Webex API's CreateDirectMessageAsync method.
        /// </summary>
        /// <param name="toPersonOrEmail">Id or email of message recipient.</param>
        /// <param name="text">Text of the message.</param>
        /// <param name="files">List of files attached to the message.</param>
        /// <returns>The created message id.</returns>
        public async Task<string> CreateMessageAsync(string toPersonOrEmail, string text, IList<Uri> files = null)
        {
            var webexResponse = await _api.CreateDirectMessageAsync(toPersonOrEmail, text, files).ConfigureAwait(false);

            return webexResponse.Data.Id;
        }

        /// <summary>
        /// Wraps Webex API's DeleteMessageAsync method.
        /// </summary>
        /// <param name="messageId">The id of the message to be deleted.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task DeleteMessageAsync(string messageId)
        {
            await _api.DeleteMessageAsync(messageId, default).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Webex API's GetMeAsync method.
        /// </summary>
        /// <returns>The <see cref="Person"/> object associated with the bot.</returns>
        public async Task<Person> GetMeAsync()
        {
            var resultPerson = await _api.GetMeAsync().ConfigureAwait(false);

            return resultPerson.GetData(false);
        }

        /// <summary>
        /// Wraps Webex API's GetMessageAsync method.
        /// </summary>
        /// <param name="messageId">Id of the message to be recovered.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>The message's data.</returns>
        public async Task<Message> GetMessageAsync(string messageId, CancellationToken? cancellationToken = null)
        {
            var message = await _api.GetMessageAsync(messageId).ConfigureAwait(false);

            return message.GetData(false);
        }

        /// <summary>
        /// Wraps Webex API's ListWebhooksAsync method.
        /// </summary>
        /// <returns>A list of Webhooks associated with the application.</returns>
        public async Task<WebhookList> ListWebhooksAsync()
        {
            var webhookList = await _api.ListWebhooksAsync().ConfigureAwait(false);

            return webhookList.GetData(false);
        }

        /// <summary>
        /// Wraps Webex API's CreateWebhookAsync method.
        /// </summary>
        /// <param name="name">Name for the webhook.</param>
        /// <param name="targetUri">Uri of the webhook.</param>
        /// <param name="resource">Event resource associated with the webhook.</param>
        /// <param name="type">Event type associated with the webhook.</param>
        /// <param name="filters">Filters for the webhook.</param>
        /// <param name="secret">Secret used to validate the webhook.</param>
        /// <returns>The created <see cref="Webhook"/>.</returns>
        public async Task<Webhook> CreateWebhookAsync(string name, Uri targetUri, EventResource resource, EventType type, IEnumerable<EventFilter> filters, string secret)
        {
            var resultWebhook = await _api.CreateWebhookAsync(name, targetUri, resource, type, null, secret).ConfigureAwait(false);

            return resultWebhook.GetData(false);
        }

        /// <summary>
        /// Wraps Webex API's DeleteWebhookAsync method.
        /// </summary>
        /// <param name="id">Id of the webhook to be deleted.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<TeamsResult<NoContent>> DeleteWebhookAsync(Webhook id)
        {
            return await _api.DeleteWebhookAsync(id).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Webex API's UpdateWebhookAsync method.
        /// </summary>
        /// <param name="webhookId">Id of the webhook to be updated.</param>
        /// <param name="name">Name for the webhook.</param>
        /// <param name="targetUri">Uri of the webhook.</param>
        /// <param name="secret">Secret used to validate the webhook.</param>
        /// <returns>The updated <see cref="Webhook"/>.</returns>
        public async Task<Webhook> UpdateWebhookAsync(string webhookId, string name, Uri targetUri, string secret)
        {
            var resultWebhook = await _api.UpdateWebhookAsync(webhookId, name, targetUri, secret).ConfigureAwait(false);

            return resultWebhook.GetData(false);
        }
    }
}
