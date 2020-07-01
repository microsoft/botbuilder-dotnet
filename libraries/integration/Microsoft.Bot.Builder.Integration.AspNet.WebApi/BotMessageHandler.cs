// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Integration.AspNet.WebApi.Handlers
{
    /// <summary>
    /// A handler to process incoming http requests via using an adapter.
    /// </summary>
    public sealed class BotMessageHandler : BotMessageHandlerBase
    {
        /// <summary>
        /// The route name for the message handler.
        /// </summary>
        public static readonly string RouteName = "BotFramework - Message Handler";

        /// <summary>
        /// Initializes a new instance of the <see cref="BotMessageHandler"/> class.
        /// </summary>
        /// <param name="adapter">An instance of <see cref="IAdapterIntegration"/>.</param>
        public BotMessageHandler(IAdapterIntegration adapter)
            : base(adapter)
        {
        }

        /// <summary>
        /// Deserializes the incoming request using a BotMessageHandler, processes it with an <see cref="IAdapterIntegration"/>
        /// and returns an <see cref="InvokeResponse"/>.
        /// </summary>
        /// <param name="request">A <see cref="HttpRequestMessage"/>.</param>
        /// <param name="adapter">An instance of <see cref="IAdapterIntegration"/>.</param>
        /// <param name="botCallbackHandler">An instance of <see cref="BotCallbackHandler"/>.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/>.</param>
        /// <returns>An <see cref="InvokeResponse"/> returned from the adapter.</returns>
        protected override async Task<InvokeResponse> ProcessMessageRequestAsync(HttpRequestMessage request, IAdapterIntegration adapter, BotCallbackHandler botCallbackHandler, CancellationToken cancellationToken)
        {
            var activity = await request.Content.ReadAsAsync<Activity>(BotMessageHandlerBase.BotMessageMediaTypeFormatters, cancellationToken).ConfigureAwait(false);

            var invokeResponse = await adapter.ProcessActivityAsync(
                request.Headers.Authorization?.ToString(),
                activity,
                botCallbackHandler,
                cancellationToken).ConfigureAwait(false);

            return invokeResponse;
        }
    }
}
