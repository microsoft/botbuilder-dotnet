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
    public interface IWebexClient
    {
        void CreateClient(string accessToken);

        Task<string> CreateMessageAsync(string toPersonOrEmail, string text);

        Task DeleteMessageAsync(string activityId);

        Task<TeamsResult<Person>> GetMeAsync();

        Task<TeamsResult<Message>> GetMessageAsync(string messageId, CancellationToken? cancellationToken = null);

        Task<TeamsListResult<WebhookList>> ListWebhooksAsync();

        Task<TeamsResult<NoContent>> DeleteWebhookAsync(Webhook id);

        Task<TeamsResult<Webhook>> UpdateWebhookAsync(string webhookId, string name, Uri targetUri, string secret);

        Task<TeamsResult<Webhook>> CreateWebhookAsync(string name, Uri targetUri, EventResource resource, EventType type, IEnumerable<EventFilter> filters, string secret);
    }
}
