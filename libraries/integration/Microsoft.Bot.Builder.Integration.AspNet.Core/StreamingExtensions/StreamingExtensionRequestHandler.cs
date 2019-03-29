// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Protocol;
using Microsoft.Bot.Protocol.WebSockets;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.StreamingExtensions
{
    internal class StreamingExtensionRequestHandler : RequestHandler
    {
        public StreamingExtensionRequestHandler()
        {
        }

        public WebSocketServer Server { get; set; }

        public IBot Bot { get; set; }

        /// <summary>
        /// Processes incoming requests and returns the response, if any.
        /// </summary>
        /// <param name="request">A ReceiveRequest from the connected channel.</param>
        /// <returns>A response created by the BotFrameworkStreamingExtensionsAdapter.</returns>
        public override async Task<Response> ProcessRequestAsync(ReceiveRequest request)
        {
            var response = new Response();

            var body = request.ReadBodyAsString().Result;

            if (string.IsNullOrEmpty(body) || request.Streams?.Count > 0)
            {
                response.StatusCode = 400;
                return response;
            }

            if (request.Streams.Where(x => x.Type != "application/json; charset=utf8").Any())
            {
                response.StatusCode = 406;
                return response;
            }

            try
            {
                var activity = JsonConvert.DeserializeObject<Activity>(body, SerializationSettings.DefaultDeserializationSettings);
                var adapter = new BotFrameworkStreamingExtensionsAdapter(Server);
                var invokeResponse = await adapter.ProcessActivityAsync(activity, new BotCallbackHandler(this.Bot.OnTurnAsync), CancellationToken.None).ConfigureAwait(false);

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
                response.StatusCode = 500;
            }

            return response;
        }
    }
}
