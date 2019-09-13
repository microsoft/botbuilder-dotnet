// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.StreamingExtensions;
using Microsoft.Bot.StreamingExtensions.Transport;
using Microsoft.Bot.StreamingExtensions.Transport.NamedPipes;
using Microsoft.Bot.StreamingExtensions.Transport.WebSockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.Bot.Builder.StreamingExtensions
{
    public class StreamingHttpClient : HttpClient
    {
        private readonly ILogger _logger;
        private Dictionary<Guid, IStreamingTransportServer> _transportServers;

        public StreamingHttpClient(ILogger logger = null)
        {
            this._logger = logger ?? NullLogger.Instance;
        }

        public override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            var streamingRequest = new StreamingRequest
            {
                Path = request.RequestUri.OriginalString.Substring(request.RequestUri.OriginalString.IndexOf("/v3")),
                Verb = request.Method.ToString(),
            };
            streamingRequest.SetBody(request.Content);

            return await this.SendRequestAsync<HttpResponseMessage>(streamingRequest, cancellationToken).ConfigureAwait(false);
        }

        public void AddConnection(string pipeName, IRequestHandler requestHandler)
        {
            var server = new NamedPipeServer(pipeName, requestHandler);
            _transportServers.Add(server.)
        }

        public void AddConnection(WebSocket socket, IRequestHandler requestHandler)
        {
            this._server = new WebSocketServer(socket, requestHandler);
        }

        public IStreamingTransportServer GetConnection(Guid id)
        {
            try
            {
                IStreamingTransportServer connection;
                _transportServers.TryGetValue(id, out connection);

                return connection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                throw;
            }
        }

        public async Task<ReceiveResponse> SendAsync(StreamingRequest streamingRequest, CancellationToken cancellationToken = default) => await this._server.SendAsync(streamingRequest, cancellationToken).ConfigureAwait(false);

        private async Task<T> SendRequestAsync<T>(StreamingRequest request, CancellationToken cancellation = default)
        {
            try
            {
                var serverResponse = await this._server.SendAsync(request, cancellation).ConfigureAwait(false);

                if (serverResponse.StatusCode == (int)HttpStatusCode.OK)
                {
                    return serverResponse.ReadBodyAsJson<T>();
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex.Message);
            }

            return default;
        }
    }
}
