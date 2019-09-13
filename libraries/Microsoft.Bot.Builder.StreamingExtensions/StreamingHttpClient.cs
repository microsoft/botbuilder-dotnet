// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
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

        public void AddConnection(string pipeName, IRequestHandler requestHandler)
        {
            var server = new NamedPipeServer(pipeName, requestHandler);
            _transportServers.Add(server.Id, server);

            // The task that begins with starting the server listening doesn't complete until the server stops listening, so we don't want to await it.
            server.StartAsync();
            server.Disconnected += Server_Disconnected;
        }

        public void AddConnection(WebSocket socket, string connectionBaseUrl, IRequestHandler requestHandler)
        {
            var server = new WebSocketServer(socket, connectionBaseUrl, requestHandler);
            _transportServers.Add(server.Id, server);

            // The task that begins with starting the server listening doesn't complete until the server stops listening, so we don't want to await it.
            server.StartAsync();
            server.Disconnected += Server_Disconnected;
        }

        /// <summary>
        /// Attempts to find an existing connection by ID.
        /// </summary>
        /// <param name="id">The guid identifying the connection to find.</param>
        /// <returns>A connection with the specified id if one exists, otherwise null.</returns>
        public IStreamingTransportServer GetConnection(Guid id)
        {
            try
            {
                IStreamingTransportServer connection = null;
                _transportServers.TryGetValue(id, out connection);

                return connection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                throw;
            }
        }

        /// <summary>
        /// Attempts to find an existing connection to an instance of the specified remote service.
        /// Useful to avoid opening multiple connections to the same service for calls that do not
        /// need to be bound to a specific connection.
        /// </summary>
        /// <param name="baseUrl">The base URL of the remote service to search for existing connections with.</param>
        /// <returns>A connection if one exists, otherwise null.</returns>
        public IStreamingTransportServer FindConnection(string baseUrl)
        {
            try
            {
                return _transportServers.Values.Where(x => x.RemoteHost.ToLowerInvariant() == baseUrl.ToLowerInvariant()).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                throw;
            }
        }

        public async Task<ReceiveResponse> SendAsync(StreamingRequest streamingRequest, CancellationToken cancellationToken = default) => await this._server.SendAsync(streamingRequest, cancellationToken).ConfigureAwait(false);

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

        private void Server_Disconnected(object sender, DisconnectedEventArgs e)
        {
            var id = (sender as IStreamingTransportServer).Id;
            _transportServers.Remove(id);
            _logger.LogInformation("De-registered connection " + id + " after receiving disconnection event: " + e.Reason);
        }
    }
}
