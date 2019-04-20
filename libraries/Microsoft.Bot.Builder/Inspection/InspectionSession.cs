// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder
{
    public class InspectionSession
    {
        private readonly ConversationReference _conversationReference;
        private readonly MicrosoftAppCredentials _credentials;
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public InspectionSession(ConversationReference conversationReference, MicrosoftAppCredentials credentials, HttpClient httpClient, ILogger logger)
        {
            _conversationReference = conversationReference;
            _credentials = credentials;
            _httpClient = httpClient;
            _logger = logger;
        }

        public Task<bool> SendAsync(Activity activity, CancellationToken cancellationToken)
        {
            activity.ChannelId = _conversationReference.ChannelId;
            activity.ServiceUrl = _conversationReference.ServiceUrl;
            if (activity.From == null)
            {
                activity.From = _conversationReference.Bot;
            }

            if (activity.Recipient == null)
            {
                activity.Recipient = _conversationReference.User;
            }

            if (activity.Conversation == null)
            {
                activity.Conversation = _conversationReference.Conversation;
            }

            return SendToConversationAsync(activity, _conversationReference, cancellationToken);
        }

        protected async Task<bool> SendToConversationAsync(Activity activity, ConversationReference conversationReference, CancellationToken cancellationToken)
        {
            if (activity.Timestamp == null)
            {
                activity.Timestamp = DateTime.UtcNow;
            }

            if (activity.ChannelId != conversationReference.ChannelId && activity.RelatesTo == null)
            {
                activity.RelatesTo = activity.GetConversationReference();
            }

            activity.ChannelId = conversationReference.ChannelId;
            activity.Conversation = conversationReference.Conversation;
            activity.ServiceUrl = conversationReference.ServiceUrl;

            var connectorClient = new ConnectorClient(new Uri(conversationReference.ServiceUrl), _credentials, _httpClient);

            try
            {
                var resourceResponse = await connectorClient.Conversations.SendToConversationAsync(activity, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception err)
            {
                _logger.LogWarning($"Exception sending to inspection endpoint {err.ToString()}");
                return false;
            }

            return true;
        }
    }
}
