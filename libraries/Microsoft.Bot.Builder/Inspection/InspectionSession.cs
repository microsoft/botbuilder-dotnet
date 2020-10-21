// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
    internal class InspectionSession : IDisposable
    {
        private readonly ConnectorClient _connectorClient;
        private readonly ConversationReference _conversationReference;
        private readonly ILogger _logger;

        public InspectionSession(ConversationReference conversationReference, MicrosoftAppCredentials credentials, HttpClient httpClient, ILogger logger)
        {
            _conversationReference = conversationReference;
            _logger = logger;
            _connectorClient = new ConnectorClient(new Uri(_conversationReference.ServiceUrl), credentials, httpClient, disposeHttpClient: httpClient == null);
        }

        public async Task<bool> SendAsync(Activity activity, CancellationToken cancellationToken)
        {
            activity.ApplyConversationReference(_conversationReference);

            try
            {
                await _connectorClient.Conversations.SendToConversationAsync(activity, cancellationToken).ConfigureAwait(false);
            }
#pragma warning disable CA1031 // Do not catch general exception types (we just log the exception in this case and return false)
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                _logger.LogWarning($"Exception '{ex}' while attempting to call Emulator for inspection, check it is running, and you have correct credentials in the Emulator and the InspectionMiddleware.");
                return false;
            }

            return true;
        }

        public void Dispose()
        {
            _connectorClient?.Dispose();
        }
    }
}
