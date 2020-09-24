// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace Microsoft.Bot.Streaming.Transport
{
    /// <summary>
    /// Used to pull raw data from the wire, via a given transport.
    /// </summary>
    public interface ITransportReceiver : ITransport
    {
        /// <summary>
        /// Called to receive data from the wire transport.
        /// </summary>
        /// <param name="buffer">A buffer to receive data into.</param>
        /// <param name="offset">The location within the buffer to begin writing.</param>
        /// <param name="count">The maximum amount of bytes to write to the buffer.</param>
        /// <returns>The number of bytes written to the buffer.</returns>
        Task<int> ReceiveAsync(byte[] buffer, int offset, int count);
    }
}
