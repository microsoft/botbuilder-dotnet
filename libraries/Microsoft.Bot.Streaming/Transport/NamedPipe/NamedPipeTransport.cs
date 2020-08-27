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

        // To detect redundant calls to dispose
        private bool _disposed;

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

        /// <summary>
        /// Disconnects the client and releases any related objects owned by the class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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

        /// <summary>
        /// Disposes objected used by the class.
        /// </summary>
        /// <param name="disposing">A Boolean that indicates whether the method call comes from a Dispose method (its value is true) or from a finalizer (its value is false).</param>
        /// <remarks>
        /// The disposing parameter should be false when called from a finalizer, and true when called from the IDisposable.Dispose method.
        /// In other words, it is true when deterministically called and false when non-deterministically called.
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // Dispose managed objects owned by the class here.
                _stream?.Dispose();
            }

            _disposed = true;
        }
    }
}
