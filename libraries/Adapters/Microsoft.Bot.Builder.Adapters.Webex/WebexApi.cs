// Copyright (c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
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

        public async Task<string> CreateMessageAsync(string toPersonOrEmail, string text)
        {
            var webexResponse = await _api.CreateDirectMessageAsync(toPersonOrEmail, text).ConfigureAwait(false);
            return webexResponse.Data.Id;
        }

        public async Task DeleteMessageAsync(string activityId)
        {
            await _api.DeleteMessageAsync(activityId, default).ConfigureAwait(false);
        }

        public async Task<TeamsResult<Person>> GetMeAsync()
        {
            return await _api.GetMeAsync().ConfigureAwait(false);
        }

        public async Task<TeamsResult<Message>> GetMessageAsync(string messageId, CancellationToken? cancellationToken = null)
        {
            return await _api.GetMessageAsync(messageId).ConfigureAwait(false);
        }

        public async Task<TeamsListResult<WebhookList>> ListWebhooksAsync()
        {
            return await _api.ListWebhooksAsync().ConfigureAwait(false);
        }

        public async Task<TeamsResult<Webhook>> CreateWebhookAsync(string name, Uri targetUri, EventResource resource, EventType type, IEnumerable<EventFilter> filters, string secret)
        {
            return await _api.CreateWebhookAsync(name, targetUri, resource, type, null, secret).ConfigureAwait(false);
        }

        public async Task<TeamsResult<NoContent>> DeleteWebhookAsync(Webhook id)
        {
            return await _api.DeleteWebhookAsync(id).ConfigureAwait(false);
        }

        public async Task<TeamsResult<Webhook>> UpdateWebhookAsync(string webhookId, string name, Uri targetUri, string secret)
        {
            return await _api.UpdateWebhookAsync(webhookId, name, targetUri, secret).ConfigureAwait(false);
        }
    }
}
