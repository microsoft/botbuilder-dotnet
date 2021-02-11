// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Streaming.Payloads;
using Microsoft.Bot.Streaming.Transport;
using Microsoft.Bot.Streaming.Utilities;

namespace Microsoft.Bot.Streaming.PayloadTransport
{
    /// <summary>
    /// On Send: queues up sends and sends them along the transport.
    /// On Receive: receives a packet header and some bytes and dispatches it to the subscriber.
    /// </summary>
    public class PayloadSender : IPayloadSender, IDisposable
    {
        private readonly SendQueue<SendPacket> _sendQueue;
        private readonly EventWaitHandle _connectedEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
        private ITransportSender _sender;
        private bool _isDisconnecting;
        private readonly byte[] _sendHeaderBuffer = new byte[TransportConstants.MaxHeaderLength];
        private readonly byte[] _sendContentBuffer = new byte[TransportConstants.MaxPayloadLength];

        // To detect redundant calls to dispose
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadSender"/> class.
        /// </summary>
        public PayloadSender()
        {
            _sendQueue = new SendQueue<SendPacket>(this.WritePacketAsync);
        }

        /// <inheritdoc/>
        public event DisconnectedEventHandler Disconnected;

        /// <inheritdoc/>
        public bool IsConnected => _sender != null;

        /// <inheritdoc/>
        public void Connect(ITransportSender sender)
        {
            if (_sender != null)
            {
                throw new InvalidOperationException("Already connected.");
            }

            _sender = sender;

            _connectedEvent.Set();
        }

        /// <inheritdoc/>
        public void SendPayload(Header header, Stream payload, bool isLengthKnown, Func<Header, Task> sentCallback)
        {
            var packet = new SendPacket()
            {
                Header = header,
                Payload = payload,
                IsLengthKnown = isLengthKnown,
                SentCallback = sentCallback,
            };
            _sendQueue.Post(packet);
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
                        if (_sender != null)
                        {
                            _sender.Close();
                            _sender.Dispose();
                            didDisconnect = true;
                        }
                    }
#pragma warning disable CA1031 // Do not catch general exception types

                    // As ITransportSender is an extension point, we don't 
                    // know what exceptions will be thrown by different implementations
                    // of ITransportSender.Close(). We do want to ensure that Disconnect doesn't
                    // stop the other resource cleanup, so we don't throw any exception.
                    // TODO: Flow ILogger all the way here and start logging these exceptions.
                    catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                    }

                    _sender = null;

                    if (didDisconnect)
                    {
                        _connectedEvent.Reset();
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
                _sender?.Dispose();
                _sendQueue?.Dispose();
                _connectedEvent?.Dispose();
            }

            _disposed = true;
        }

        private async Task WritePacketAsync(SendPacket packet)
        {
            _connectedEvent.WaitOne();

            try
            {
                // determine if we know the payload length and end
                if (!packet.IsLengthKnown)
                {
                    var count = await packet.Payload.ReadAsync(_sendContentBuffer, 0, TransportConstants.MaxPayloadLength).ConfigureAwait(false);
                    packet.Header.PayloadLength = count;
                    packet.Header.End = count == 0;
                }

                int length;

                var headerLength = HeaderSerializer.Serialize(packet.Header, _sendHeaderBuffer, 0);

                // Send: Packet Header
                length = await _sender.SendAsync(_sendHeaderBuffer, 0, headerLength).ConfigureAwait(false);
                if (length == 0)
                {
                    throw new TransportDisconnectedException();
                }

                var offset = 0;

                // Send content in chunks
                if (packet.Header.PayloadLength > 0 && packet.Payload != null)
                {
                    // If we already read the buffer, send that
                    // If we did not, read from the stream until we've sent that amount
                    if (!packet.IsLengthKnown)
                    {
                        // Send: Packet content
                        length = await _sender.SendAsync(_sendContentBuffer, 0, packet.Header.PayloadLength).ConfigureAwait(false);
                        if (length == 0)
                        {
                            throw new TransportDisconnectedException();
                        }
                    }
                    else
                    {
                        do
                        {
                            var count = Math.Min(packet.Header.PayloadLength - offset, TransportConstants.MaxPayloadLength);

                            // copy the stream to the buffer
                            count = await packet.Payload.ReadAsync(_sendContentBuffer, 0, count).ConfigureAwait(false);

                            // Send: Packet content
                            length = await _sender.SendAsync(_sendContentBuffer, 0, count).ConfigureAwait(false);
                            if (length == 0)
                            {
                                throw new TransportDisconnectedException();
                            }

                            offset += count;
                        }
                        while (offset < packet.Header.PayloadLength);
                    }
                }

                if (packet.SentCallback != null)
                {
                    Background.Run(() => packet.SentCallback(packet.Header));
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                var disconnectedArgs = new DisconnectedEventArgs()
                {
                    Reason = e.Message,
                };
                Disconnect(disconnectedArgs);
            }
        }
    }
}
