// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Bot.Streaming.Payloads;
using Microsoft.Bot.Streaming.Transport;
using Microsoft.Bot.Streaming.Utilities;

namespace Microsoft.Bot.Streaming.PayloadTransport
{
    /// <summary>
    /// PayloadReceivers subscribe to incoming streams and manage the consumption of raw data as it comes in.
    /// </summary>
    public class PayloadReceiver : IPayloadReceiver, IDisposable
    {
        private Func<Header, Stream> _getStream;
        private Action<Header, Stream, int> _receiveAction;
        private ITransportReceiver _receiver;
        private bool _isDisconnecting = false;
        private readonly byte[] _receiveHeaderBuffer = new byte[TransportConstants.MaxHeaderLength];
        private readonly byte[] _receiveContentBuffer = new byte[TransportConstants.MaxPayloadLength];

        // To detect redundant calls to dispose
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadReceiver"/> class.
        /// </summary>
        public PayloadReceiver()
        {
        }

        /// <inheritdoc/>
        public event DisconnectedEventHandler Disconnected;

        /// <inheritdoc/>
        public bool IsConnected => _receiver != null;

        /// <inheritdoc/>
        public void Connect(ITransportReceiver receiver)
        {
            if (_receiver != null)
            {
                throw new InvalidOperationException("Already connected.");
            }

            _receiver = receiver;

            RunReceive();
        }

        /// <inheritdoc/>
        public void Subscribe(
            Func<Header, Stream> getStream,
            Action<Header, Stream, int> receiveAction)
        {
            _getStream = getStream;
            _receiveAction = receiveAction;
        }

        /// <inheritdoc/>
        public void Disconnect(DisconnectedEventArgs e = null)
        {
            var didDisconnect = false;
            if (!_isDisconnecting)
            {
                _isDisconnecting = true;
                try
                {
                    try
                    {
                        if (_receiver != null)
                        {
                            _receiver.Close();
                            _receiver.Dispose();
                            didDisconnect = true;
                        }
                    }
#pragma warning disable CA1031 // Do not catch general exception types

                    // As ITransportReceiver is an extension point, we don't 
                    // know what exceptions will be thrown by different implementations
                    // of ITransportReceiver.Close(). We do want to ensure that Disconnect doesn't
                    // stop the other resource cleanup, so we don't throw any exception.
                    // TODO: Flow ILogger all the way here and start logging these exceptions.
                    catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                    }

                    _receiver = null;

                    if (didDisconnect)
                    {
                        Disconnected?.Invoke(this, e ?? DisconnectedEventArgs.Empty);
                    }
                }
                finally
                {
                    _isDisconnecting = false;
                }
            }
        }

        /// <summary>
        /// Disposes the object and releases any related objects owned by the class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
                _receiver?.Dispose();
            }

            _disposed = true;
        }

        private void RunReceive() => Background.Run(ReceivePacketsAsync);

        private async Task ReceivePacketsAsync()
        {
            bool isClosed = false;
            DisconnectedEventArgs disconnectArgs = null;

            while (_receiver != null && _receiver.IsConnected && !isClosed)
            {
                // receive a single packet
                try
                {
                    // read the header
                    int headerOffset = 0;
                    int length;
                    while (headerOffset < TransportConstants.MaxHeaderLength)
                    {
                        length = await _receiver.ReceiveAsync(_receiveHeaderBuffer, headerOffset, TransportConstants.MaxHeaderLength - headerOffset).ConfigureAwait(false);
                        if (length == 0)
                        {
                            throw new TransportDisconnectedException("Stream closed while reading header bytes");
                        }

                        headerOffset += length;
                    }

                    // deserialize the bytes into a header
                    var header = HeaderSerializer.Deserialize(_receiveHeaderBuffer, 0, TransportConstants.MaxHeaderLength);

                    // read the payload
                    var contentStream = _getStream(header);

                    var buffer = PayloadTypes.IsStream(header) ?
                        new byte[header.PayloadLength] :
                        _receiveContentBuffer;

                    int offset = 0;

                    if (header.PayloadLength > 0)
                    {
                        do
                        {
                            // read in chunks
                            int count = Math.Min(header.PayloadLength - offset, TransportConstants.MaxPayloadLength);

                            // read the content
                            length = await _receiver.ReceiveAsync(buffer, offset, count).ConfigureAwait(false);
                            if (length == 0)
                            {
                                throw new TransportDisconnectedException("Stream closed while reading payload bytes");
                            }

                            if (contentStream != null)
                            {
                                // write chunks to the contentStream if it's not a stream type
                                if (!PayloadTypes.IsStream(header))
                                {
                                    await contentStream.WriteAsync(buffer, offset, length).ConfigureAwait(false);
                                }
                            }

                            offset += length;
                        }
                        while (offset < header.PayloadLength);

                        // give the full payload buffer to the contentStream if it's a stream
                        if (contentStream != null && PayloadTypes.IsStream(header))
                        {
                            ((PayloadStream)contentStream).GiveBuffer(buffer, length);
                        }
                    }

                    _receiveAction(header, contentStream, offset);
                }
                catch (TransportDisconnectedException de)
                {
                    isClosed = true;
                    disconnectArgs = new DisconnectedEventArgs()
                    {
                        Reason = de.Reason,
                    };
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    isClosed = true;
                    disconnectArgs = new DisconnectedEventArgs()
                    {
                        Reason = e.Message,
                    };
                }
            }

            Disconnect(disconnectArgs);
        }
    }
}
