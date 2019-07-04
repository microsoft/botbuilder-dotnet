// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.StreamingExtensions.Payloads
{
    internal class PayloadStream : Stream
    {
        private readonly PayloadStreamAssembler _assembler;
        private readonly Queue<byte[]> _bufferQueue = new Queue<byte[]>();

        private readonly SemaphoreSlim dataAvailable = new SemaphoreSlim(0, int.MaxValue);
        private readonly object syncLock = new object();
        private long _producerLength = 0;       // total length
        private long _consumerPosition = 0;     // read position

        private byte[] _active = null;
        private int _activeOffset = 0;

        private bool _end = false;

        internal PayloadStream(PayloadStreamAssembler assembler)
        {
            _assembler = assembler;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Position { get => _consumerPosition; set => throw new NotSupportedException(); }

        public override long Length => _producerLength;

        public override void Flush()
        {
            /*
             No-op. PayloadStreams should never be flushed, so
             we override Stream's Flush to make sure no caller
             attempts to flush a PayloadStream.
            */
        }

        public override void SetLength(long value) => throw new NotSupportedException();

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (_end)
            {
                return 0;
            }

            if (_active == null)
            {
                await dataAvailable.WaitAsync(cancellationToken).ConfigureAwait(false);

                lock (syncLock)
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

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var copy = new byte[count];
            Array.Copy(buffer, offset, copy, 0, count);
            GiveBuffer(copy, count);
            return Task.CompletedTask;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            var copy = new byte[count];
            Array.Copy(buffer, offset, copy, 0, count);
            GiveBuffer(copy, count);
        }

        public void Cancel()
        {
            if (_assembler != null)
            {
                _assembler.Close();
            }

            DoneProducing();
        }

        public override int Read(byte[] buffer, int offset, int count) => throw new NotImplementedException();

        internal void GiveBuffer(byte[] buffer, int count)
        {
            lock (syncLock)
            {
                _bufferQueue.Enqueue(buffer);
                _producerLength += count;
            }

            dataAvailable.Release();
        }

        internal void DoneProducing() => GiveBuffer(Array.Empty<byte>(), 0);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Cancel();
            }

            base.Dispose(disposing);
        }
    }
}
