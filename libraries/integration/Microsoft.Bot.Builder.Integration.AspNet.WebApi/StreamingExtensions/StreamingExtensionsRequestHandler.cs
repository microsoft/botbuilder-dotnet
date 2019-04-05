// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Protocol;
using Microsoft.Bot.Protocol.WebSockets;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Integration.AspNet.WebApi.StreamingExtensions
{
    internal class StreamingExtensionsRequestHandler : RequestHandler
    {
        public StreamingExtensionsRequestHandler()
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

            var body = await request.ReadBodyAsString().ConfigureAwait(false);

            if (string.IsNullOrEmpty(body) || request.Streams?.Count == 0)
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                return response;
            }

            if (request.Streams.Where(x => x.Type != "application/json; charset=utf-8").Any())
            {
                response.StatusCode = (int)HttpStatusCode.NotAcceptable;
                return response;
            }

            try
            {
                var activity = JsonConvert.DeserializeObject<Activity>(body, SerializationSettings.DefaultDeserializationSettings);
                var adapter = new BotFrameworkStreamingExtensionsAdapter(Server);
                var invokeResponse = await adapter.ProcessActivityAsync(activity, new BotCallbackHandler(this.Bot.OnTurnAsync), CancellationToken.None).ConfigureAwait(false);

                if (invokeResponse == null)
                {
                    response.StatusCode = (int)HttpStatusCode.OK;
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
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

            return response;
        }
    }
}
