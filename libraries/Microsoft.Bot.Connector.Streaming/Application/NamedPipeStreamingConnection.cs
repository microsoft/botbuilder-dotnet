// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO.Pipelines;
using Microsoft.Bot.Connector.Streaming.Transport;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Connector.Streaming.Application
{
    /// <summary>
    /// Default implementation of <see cref="StreamingConnection"/> for NamedPipe transport.
    /// </summary>
    public class NamedPipeStreamingConnection : StreamingConnection
    {
        private readonly string _pipeName;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedPipeStreamingConnection"/> class.
        /// </summary>
        /// <param name="pipeName">The name of the named pipe.</param>
        /// <param name="logger"><see cref="ILogger"/> for the connection.</param>
        public NamedPipeStreamingConnection(string pipeName, ILogger logger)
            : base(logger)
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
