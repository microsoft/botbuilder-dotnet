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
    internal class InspectionSession
    {
        private readonly ConversationReference _conversationReference;
        private readonly ILogger _logger;
        private readonly ConnectorClient _connectorClient;

        public InspectionSession(ConversationReference conversationReference, MicrosoftAppCredentials credentials, HttpClient httpClient, ILogger logger)
        {
            _conversationReference = conversationReference;
            _logger = logger;
            _connectorClient = new ConnectorClient(new Uri(_conversationReference.ServiceUrl), credentials, httpClient);
        }

        public async Task<bool> SendAsync(Activity activity, CancellationToken cancellationToken)
        {
            activity.ApplyConversationReference(_conversationReference);

            try
            {
                var resourceResponse = await _connectorClient.Conversations.SendToConversationAsync(activity, cancellationToken).ConfigureAwait(false);
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
