using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Protocol.Payloads;
using Microsoft.Bot.Protocol.Utilities;

namespace Microsoft.Bot.Protocol
{
    public class ConcurrentStream : Stream
    {
        private readonly ContentStreamAssembler _assembler;
        private readonly Queue<byte[]> _bufferQueue = new Queue<byte[]>();

        private readonly SemaphoreSlim dataAvailable = new SemaphoreSlim(0, Int32.MaxValue);
        private readonly object syncLock = new object();

        private long _producerLength = 0;       // total length
        private long _consumerPosition = 0;     // read position

        private byte[] _active = null;
        private int _activeOffset = 0;

        private bool _end = false;

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => _producerLength;

        public override long Position { get => _consumerPosition; set => throw new NotSupportedException(); }
        
        internal ConcurrentStream(ContentStreamAssembler assembler)
        {
            _assembler = assembler;
        }
        
        public override void Flush()
        {
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }
        
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
        
        /// <summary>
        /// Only 1 thread should be calling Read at a time
        /// </summary>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_end)
            {
                return 0;
            }

            if (_active == null)
            {
                dataAvailable.Wait();

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

        internal void GiveBuffer(byte[] buffer, int count)
        {
            lock (syncLock)
            {
                _bufferQueue.Enqueue(buffer);
                _producerLength += count;
            }
            dataAvailable.Release();
        }

        internal void DoneProducing()
        {
            GiveBuffer(Array.Empty<byte>(), 0);
        }
        
        public void Cancel()
        {
            if (_assembler != null)
            {
                _assembler.Close();
            }

            DoneProducing();
        }

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
