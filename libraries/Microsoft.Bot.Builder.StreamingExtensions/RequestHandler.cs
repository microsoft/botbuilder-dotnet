// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Bot.StreamingExtensions;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder.StreamingExtensions
{
    internal class RequestHandler : IRequestHandler
    {
        private readonly DirectLineAdapter _adapter;
        private readonly StreamingHttpClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestHandler"/> class.
        /// </summary>
        /// <param name="adapter">The BotFramework adapter to use to process activities.</param>
        /// <param name="client">The <see cref="StreamingHttpClient"/> this request handler is bound to.</param>
        public RequestHandler(DirectLineAdapter adapter, StreamingHttpClient client)
        {
            this._adapter = adapter;
            this._client = client;
        }

        /// <summary>
        /// Pass through to the adapter's ProcessRequestAsync method, suitable for introducing custom handling of incoming messages.
        /// </summary>
        /// <param name="request">A request sent up from the streaming extension library <see cref="ProtocolAdapter"/>.</param>
        /// <param name="logger">A logger.</param>
        /// <param name="context">An optional context that can be used contain all handling of a request to within the same context. Unused in the basic bot implementation.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that resolves to a <see cref="StreamingResponse"/>.</returns>
        public async Task<StreamingResponse> ProcessRequestAsync(ReceiveRequest request, ILogger logger, object context = null, CancellationToken cancellationToken = default)
        {
            return await this._adapter.ProcessRequestAsync(request, logger, context, cancellationToken);
        }

        private void RegisterConversationWithConnection(ReceiveRequest request)
        {
            throw new NotImplementedException();

            // var activity = request.ReadBodyAsJson<Activity>();

            // var channelID = activity.ChannelId;
            // var conversationID = activity.Conversation.Id;

            // this._client.
        }
    }
}
