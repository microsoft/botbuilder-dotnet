using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.StreamingExtensions.Transport;

namespace Microsoft.Bot.StreamingExtensions.UnitTests.Mocks
{
    public class MockTransportReceiver : ITransportReceiver
    {
        public byte[] Buffer { get; set; }

        public int Offset { get; set; }

        public bool IsConnected => true;

        public MockTransportReceiver(byte[] buffer)
        {
            Buffer = buffer;
        }

        public Task<int> ReceiveAsync(byte[] buffer, int offset, int count)
        {
            int availableBytes = Math.Min(Buffer.Length - offset, count);
            Array.Copy(Buffer, Offset, buffer, offset, availableBytes);
            Offset += availableBytes;
            return Task.FromResult(availableBytes);
        }

        public void Close()
        {
        }

        public void Dispose()
        {
        }
    }
}
