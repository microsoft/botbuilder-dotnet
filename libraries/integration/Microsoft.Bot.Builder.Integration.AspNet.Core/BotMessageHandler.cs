// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.Handlers
{
    /// <summary>
    /// A handler to process incoming http requests via using an adapter.
    /// </summary>
    public class BotMessageHandler : BotMessageHandlerBase
    {
        /// <summary>
        /// Deserializes the incoming request using a BotMessageHandler, processes it with an <see cref="IAdapterIntegration"/>
        /// and returns an <see cref="InvokeResponse"/>.
        /// </summary>
        /// <param name="request">A <see cref="HttpRequest"/>.</param>
        /// <param name="adapter">An instance of <see cref="IAdapterIntegration"/>.</param>
        /// <param name="botCallbackHandler">An instance of <see cref="BotCallbackHandler"/>.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/>.</param>
        /// <returns>An <see cref="InvokeResponse"/> returned from the adapter.</returns>
        protected override async Task<InvokeResponse> ProcessMessageRequestAsync(HttpRequest request, IAdapterIntegration adapter, BotCallbackHandler botCallbackHandler, CancellationToken cancellationToken)
        {
            Activity activity;

            using (var memoryStream = new MemoryStream())
            {
                await request.Body.CopyToAsync(memoryStream).ConfigureAwait(false);

                // In the case of request buffering being enabled, we could possibly receive a Stream that needs its position reset,
                // but we can't _blindly_ reset in case buffering hasn't been enabled since we'll be working with a non-seekable Stream
                // in that case which will throw a NotSupportedException
                if (request.Body.CanSeek)
                {
                    request.Body.Position = 0;
                }

                memoryStream.Position = 0;

                // Get the request body and deserialize to the Activity object.
                // NOTE: We explicitly leave the stream open here so others can still access it (in case buffering was enabled); ASP.NET runtime will always dispose of it anyway
                using (var bodyReader = new JsonTextReader(new StreamReader(memoryStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true)))
                {
                    activity = BotMessageSerializer.Deserialize<Activity>(bodyReader);
                }
            }

            var invokeResponse = await adapter.ProcessActivityAsync(
                request.Headers["Authorization"],
                activity,
                botCallbackHandler,
                cancellationToken).ConfigureAwait(false);

            return invokeResponse;
        }
    }
}
