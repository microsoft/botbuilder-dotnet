// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Streaming.Payloads
{
    /// <summary>
    /// An extension of <see cref="Stream"/> that operates in conjunction with a <see cref="PayloadStreamAssembler"/> to convert raw bytes into a consumable form.
    /// </summary>
    public class PayloadStream : Stream
    {
        private readonly PayloadStreamAssembler _assembler;
        private readonly Queue<byte[]> _bufferQueue = new Queue<byte[]>();

        private readonly SemaphoreSlim _dataAvailable = new SemaphoreSlim(0, int.MaxValue);
        private readonly object _syncLock = new object();
        private long _producerLength;       // total length
        private long _consumerPosition;     // read position

        private byte[] _active;
        private int _activeOffset;

        private bool _end;

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadStream"/> class.
        /// </summary>
        /// <param name="assembler">The <see cref="PayloadStreamAssembler"/> to use when constructing this stream.</param>
        public PayloadStream(PayloadStreamAssembler assembler)
        {
            _assembler = assembler;
        }

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => false;

        /// <inheritdoc/>
        public override bool CanWrite => true;

        /// <inheritdoc/>
        public override long Position { get => _consumerPosition; set => throw new NotSupportedException(); }

        /// <inheritdoc/>
        public override long Length => _producerLength;

        /// <summary>
        /// No-op. PayloadStreams should never be flushed, so we override Stream's Flush to make sure no caller attempts to flush a PayloadStream.
        /// </summary>
        public override void Flush()
        {
        }

        /// <summary>
        /// Not supported. Throws <see cref="NotSupportedException"/>.
        /// </summary>
        /// <param name="value">No-op.</param>
        public override void SetLength(long value) => throw new NotSupportedException();

        /// <summary>
        /// Not supported. Throws <see cref="NotSupportedException"/>.
        /// </summary>
        /// <param name="offset">No-op.</param>
        /// <param name="origin">No-op also.</param>
        /// <returns>Throws <see cref="NotSupportedException"/>.</returns>
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (_end)
            {
                return 0;
            }

            if (_active == null)
            {
                await _dataAvailable.WaitAsync(cancellationToken).ConfigureAwait(false);

                lock (_syncLock)
                {
                    _active = _bufferQueue.Dequeue();
                }
            }

            var availableCount = (int)Math.Min(_active.Length - _activeOffset, count);
            Array.Copy(_active, _activeOffset, buffer, offset, availableCount);
            _activeOffset += availableCount;
            _consumerPosition += availableCount;

            if (_activeOffset >= _active.Length)
            {
                _active = null;
                _activeOffset = 0;
            }

            if (_assembler != null && _consumerPosition >= _assembler.ContentLength)
            {
                _end = true;
            }

            return availableCount;
        }

        /// <inheritdoc/>
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var copy = new byte[count];
            Array.Copy(buffer, offset, copy, 0, count);
            GiveBuffer(copy, count);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            var copy = new byte[count];
            Array.Copy(buffer, offset, copy, 0, count);
            GiveBuffer(copy, count);
        }

        /// <summary>
        /// Closes the connected <see cref="PayloadStreamAssembler"/> and ends production.
        /// </summary>
        public void Cancel()
        {
            _assembler?.Close();
            DoneProducing();
        }

        /// <summary>
        /// This function is called by StreamReader when processing streams.
        /// It will appear to have no references, but is in fact required to
        /// be implemented by StreamReader, just like Length.
        /// </summary>
        /// <param name="buffer">The buffer to read from.</param>
        /// <param name="offset">The position to begin reading from.</param>
        /// <param name="count">The amount to attempt to read.</param>
        /// <returns>The number of chunks remaining unread in the buffer.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_end)
            {
                return 0;
            }

            if (_active == null)
            {
                _dataAvailable.Wait();

                lock (_syncLock)
                {
                    _active = _bufferQueue.Dequeue();
                }
            }

            var availableCount = (int)Math.Min(_active.Length - _activeOffset, count);
            Array.Copy(_active, _activeOffset, buffer, offset, availableCount);
            _activeOffset += availableCount;
            _consumerPosition += availableCount;

            if (_activeOffset >= _active.Length)
            {
                _active = null;
                _activeOffset = 0;
            }

            if (_assembler != null && _consumerPosition >= _assembler.ContentLength)
            {
                _end = true;
            }

            return availableCount;
        }

        /// <summary>
        /// Called when production is cancelled or completed.
        /// </summary>
        public void DoneProducing() => GiveBuffer(Array.Empty<byte>(), 0);

        /// <summary>
        /// Releases the buffered data.
        /// </summary>
        /// <param name="buffer">The data buffer.</param>
        /// <param name="count">The amount of data contained in the buffer.</param>
        internal void GiveBuffer(byte[] buffer, int count)
        {
            lock (_syncLock)
            {
                _bufferQueue.Enqueue(buffer);
                _producerLength += count;
            }

            _dataAvailable.Release();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Cancel();
                _dataAvailable?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
