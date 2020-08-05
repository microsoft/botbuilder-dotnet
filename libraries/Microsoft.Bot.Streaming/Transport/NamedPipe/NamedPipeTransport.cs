// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace Microsoft.Bot.Streaming.Transport.NamedPipes
{
    /// <summary>
    /// For use when the wire transport is a Named Pipe.
    /// </summary>
    public class NamedPipeTransport : ITransportSender, ITransportReceiver
    {
        /// <summary>
        /// The suffix of the Named Pipe used for incoming data.
        /// </summary>
        public const string ServerIncomingPath = ".incoming";

        /// <summary>
        /// The suffix of the Named pipe used for outgoing data.
        /// </summary>
        public const string ServerOutgoingPath = ".outgoing";

        private readonly PipeStream _stream;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedPipeTransport"/> class.
        /// </summary>
        /// <param name="stream">The data stream over the Named Pipe.</param>
        public NamedPipeTransport(PipeStream stream)
        {
            _stream = stream;
        }

        /// <inheritdoc/>
        public bool IsConnected => _stream.IsConnected;

        /// <inheritdoc/>
        public void Close()
        {
            _stream.Close();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _stream.Dispose();
        }

        /// <inheritdoc/>
        public async Task<int> ReceiveAsync(byte[] buffer, int offset, int count)
        {
            try
            {
                if (_stream != null)
                {
                    var length = await _stream.ReadAsync(buffer, offset, count).ConfigureAwait(false);
                    return length;
                }
            }
            catch (ObjectDisposedException)
            {
                // _stream was disposed by a disconnect
            }

            return 0;
        }

        /// <inheritdoc/>
        public async Task<int> SendAsync(byte[] buffer, int offset, int count)
        {
            try
            {
                if (_stream != null)
                {
                    await _stream.WriteAsync(buffer, offset, count).ConfigureAwait(false);
                    return count;
                }
            }
            catch (ObjectDisposedException)
            {
                // _stream was disposed by a Disconnect call
            }
            catch (IOException)
            {
                // _stream was disposed by a disconnect of a broken pipe
            }

            return 0;
        }
    }
}
