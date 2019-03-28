// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Protocol;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.StreamingExtensions
{
    internal class StreamingExtensionRequestHandler : RequestHandler
    {
        public StreamingExtensionRequestHandler(BotFrameworkStreamingExtensionsAdapter adapter, IBot bot)
        {
            this.StreamingAdapter = adapter;
            this.Bot = bot;
        }

        private BotFrameworkStreamingExtensionsAdapter StreamingAdapter { get; set; }

        private IBot Bot { get; set; }

        public override async Task<Response> ProcessRequestAsync(ReceiveRequest request)
        {
            var response = new Response();

            var body = request.ReadBodyAsString().Result;

            if (string.IsNullOrEmpty(body))
            {
                response.StatusCode = 400;
                return response;
            }

            try
            {
                var activity = JsonConvert.DeserializeObject<Activity>(body, SerializationSettings.DefaultDeserializationSettings);
                var invokeResponse = await this.StreamingAdapter.ProcessActivityAsync(activity, new BotCallbackHandler(this.Bot.OnTurnAsync), CancellationToken.None).ConfigureAwait(false);

                if (invokeResponse == null)
                {
                    response.StatusCode = 200;
                }
                else
                {
                    response.StatusCode = invokeResponse.Status;
                    if (invokeResponse.Body != null)
                    {
                        response.SetBody(invokeResponse.Body);
                    }
                }

                invokeResponse = (InvokeResponse)null;
            }
            catch (Exception)
            {
                // TODO: Better exception handling.
                response.StatusCode = 403;
            }

            return response;
        }
    }
}
