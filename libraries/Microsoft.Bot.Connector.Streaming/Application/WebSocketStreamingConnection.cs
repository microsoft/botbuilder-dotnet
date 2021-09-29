// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Connector.Streaming.Session;
using Microsoft.Bot.Connector.Streaming.Transport;
using Microsoft.Bot.Streaming;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.Bot.Connector.Streaming.Application
{
    /// <summary>
    /// Default implementation of <see cref="StreamingConnection"/> for WebSocket transport.
    /// </summary>
    public class WebSocketStreamingConnection : StreamingConnection
    {
        private readonly ILogger _logger;
        private readonly HttpContext _httpContext;

        private StreamingSession _session;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketStreamingConnection"/> class.
        /// </summary>
        /// <param name="httpContext"><see cref="HttpContext"/> instance on which to accept the web socket.</param>
        /// <param name="logger"><see cref="ILogger"/> for the connection.</param>
        public WebSocketStreamingConnection(HttpContext httpContext, ILogger logger)
            : this(logger)
        {
            _httpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
        }

        internal WebSocketStreamingConnection(ILogger logger)
        {
            _logger = logger ?? NullLogger.Instance;
        }

        /// <inheritdoc/>
        public override async Task<ReceiveResponse> SendStreamingRequestAsync(StreamingRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return await _session.SendRequestAsync(request, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public override async Task ListenAsync(RequestHandler requestHandler, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (requestHandler == null)
            {
                throw new ArgumentNullException(nameof(requestHandler));
            }

            await ListenImplAsync(
                socketConnectFunc: t => t.ConnectAsync(_httpContext, CancellationToken.None),
                requestHandler: requestHandler,
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        internal async Task ListenInternalAsync(WebSocket webSocket, RequestHandler requestHandler, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (requestHandler == null)
            {
                throw new ArgumentNullException(nameof(requestHandler));
            }

            if (requestHandler == null)
            {
                throw new ArgumentNullException(nameof(requestHandler));
            }

            await ListenImplAsync(
                socketConnectFunc: t => t.ProcessSocketAsync(webSocket, cancellationToken),
                requestHandler: requestHandler,
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        private async Task ListenImplAsync(Func<WebSocketTransport, Task> socketConnectFunc, RequestHandler requestHandler, CancellationToken cancellationToken = default(CancellationToken))
        {
            var duplexPipePair = DuplexPipe.CreateConnectionPair(PipeOptions.Default, PipeOptions.Default);

            var transport = new WebSocketTransport(duplexPipePair.Application, _logger);
            var application = new TransportHandler(duplexPipePair.Transport, _logger);

            _session = new StreamingSession(requestHandler, application, _logger);

            var transportTask = socketConnectFunc(transport);

            var applicationTask = application.ListenAsync(cancellationToken);

            var tasks = new List<Task>() { transportTask, applicationTask };

            if (!Debugger.IsAttached)
            {
                tasks.Add(Task.Delay(TimeSpan.FromSeconds(10)));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}
