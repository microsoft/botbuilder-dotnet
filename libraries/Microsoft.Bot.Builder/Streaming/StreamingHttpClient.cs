// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Streaming;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.Bot.Builder.Streaming
{
    internal class StreamingHttpClient : HttpClient
    {
        private readonly StreamingRequestHandler _requestHandler;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamingHttpClient"/> class.
        /// An implementation of <see cref="HttpClient"/> that adds compatibility with streaming connections.
        /// </summary>
        /// <param name="requestHandler">The <see cref="StreamingRequestHandler"/> to send requests through.</param>
        /// <param name="logger">A logger.</param>
        public StreamingHttpClient(StreamingRequestHandler requestHandler, ILogger logger = null)
        {
            _requestHandler = requestHandler ?? throw new ArgumentNullException(nameof(requestHandler));
            _logger = logger ?? NullLogger.Instance;
        }

        public override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var streamingRequest = new StreamingRequest
            {
                Path = request.RequestUri.OriginalString.Substring(request.RequestUri.OriginalString.IndexOf("/v3", StringComparison.Ordinal)),
                Verb = request.Method.ToString(),
            };
            streamingRequest.SetBody(request.Content);

            return await SendRequestAsync<HttpResponseMessage>(streamingRequest, cancellationToken).ConfigureAwait(false);
        }

        public async Task<ReceiveResponse> SendAsync(StreamingRequest streamingRequest, CancellationToken cancellationToken = default) => await _requestHandler.SendStreamingRequestAsync(streamingRequest, cancellationToken).ConfigureAwait(false);

        private async Task<T> SendRequestAsync<T>(StreamingRequest request, CancellationToken cancellation = default)
        {
            try
            {
                var serverResponse = await _requestHandler.SendStreamingRequestAsync(request, cancellation).ConfigureAwait(false);

                if (serverResponse == null)
                {
                    throw new InvalidOperationException("Server response from streaming request is null");
                }

                if (serverResponse.StatusCode == (int)HttpStatusCode.OK)
                {
                    return serverResponse.ReadBodyAsJson<T>();
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types (we just log the exception and continue)
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                _logger.LogError(ex.ToString());
            }

            return default;
        }
    }
}
