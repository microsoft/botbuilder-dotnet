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
    public class BotMessageHandler : BotMessageHandlerBase
    {
        protected override async Task<InvokeResponse> ProcessMessageRequestAsync(HttpRequest request, IAdapterIntegration adapter, BotCallbackHandler botCallbackHandler, CancellationToken cancellationToken)
        {
            var requestBody = request.Body;

            // In the case of request buffering being enabled, we could possibly receive a Stream that needs its position reset,
            // but we can't _blindly_ reset in case buffering hasn't been enabled since we'll be working with a non-seekable Stream
            // in that case which will throw a NotSupportedException
            if (requestBody.CanSeek)
            {
                requestBody.Position = 0;
            }

            var activity = default(Activity);

            // Get the request body and deserialize to the Activity object.
            // NOTE: We explicitly leave the stream open here so others can still access it (in case buffering was enabled); ASP.NET runtime will always dispose of it anyway
            using (var bodyReader = new JsonTextReader(new StreamReader(requestBody, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true)))
            {
                activity = BotMessageHandlerBase.BotMessageSerializer.Deserialize<Activity>(bodyReader);
            }

#pragma warning disable UseConfigureAwait // Use ConfigureAwait
            var invokeResponse = await adapter.ProcessActivityAsync(
                    request.Headers["Authorization"],
                    activity,
                    botCallbackHandler,
                    cancellationToken);
#pragma warning restore UseConfigureAwait // Use ConfigureAwait

            return invokeResponse;
        }
    }
}
