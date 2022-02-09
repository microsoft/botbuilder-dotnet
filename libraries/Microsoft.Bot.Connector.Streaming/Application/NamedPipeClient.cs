// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO.Pipelines;
using Microsoft.Bot.Connector.Streaming.Transport;
using Microsoft.Bot.Streaming;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Connector.Streaming.Application
{
    /// <summary>
    /// Named pipe client.
    /// </summary>
    public class NamedPipeClient : StreamingTransportClient
    {
        private readonly string _pipeName;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedPipeClient"/> class.
        /// </summary>
        /// <param name="pipeName">The name of the named pipe that will initiate connection to a server.</param>
        /// <param name="url">The server URL to connect to.</param>
        /// <param name="requestHandler">Handler that will receive incoming requests to this client instance.</param>
        /// <param name="closeTimeOut">Optional time out for closing the client connection.</param>
        /// <param name="keepAlive">Optional spacing between keep alives for proactive disconnection detection. If null is provided, no keep alives will be sent.</param>
        /// <param name="logger"><see cref="ILogger"/> for the client.</param>
        public NamedPipeClient(string pipeName, string url, RequestHandler requestHandler, TimeSpan? closeTimeOut = null, TimeSpan? keepAlive = null, ILogger logger = null)
            : base(url, requestHandler, closeTimeOut, keepAlive, logger)
        {
            if (string.IsNullOrWhiteSpace(pipeName))
            {
                throw new ArgumentNullException(nameof(pipeName));
            }

            _pipeName = pipeName;
        }

        internal override StreamingTransport CreateStreamingTransport(IDuplexPipe application)
        {
            return new NamedPipeTransport(_pipeName, application, Logger);
        }
    }
}
