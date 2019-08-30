// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.StreamingExtensions;
using Microsoft.Bot.StreamingExtensions.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.Bot.Builder.StreamingExtensions
{
    public class StreamingHttpClient : HttpClient
    {
        private IStreamingTransportServer _server;
        private ILogger _logger;

        public StreamingHttpClient(IStreamingTransportServer server, ILogger logger = null)
        {
            this._server = server;
            _logger = logger != null ? logger : NullLogger.Instance;
        }

        public override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var streamingRequest = new StreamingRequest();
            streamingRequest.Path = request.RequestUri.OriginalString.Substring(request.RequestUri.OriginalString.IndexOf("/v3"));
            streamingRequest.Verb = request.Method.ToString();
            streamingRequest.SetBody(request.Content);

            return await SendRequestAsync<HttpResponseMessage>(streamingRequest, cancellationToken).ConfigureAwait(false);
        }

        private async Task<T> SendRequestAsync<T>(StreamingRequest request, CancellationToken cancellation = default(CancellationToken))
        {
            try
            {
                var serverResponse = await _server.SendAsync(request, cancellation).ConfigureAwait(false);

                if (serverResponse.StatusCode == (int)HttpStatusCode.OK)
                {
                    return serverResponse.ReadBodyAsJson<T>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return default(T);
        }
    }
}
