// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace Microsoft.Bot.Streaming.Transport
{
    /// <summary>
    /// Used to write raw data to the wire, via a given transport.
    /// </summary>
    public interface ITransportSender : ITransport
    {
        /// <summary>
        /// Called to send data to the wire transport.
        /// </summary>
        /// <param name="buffer">A buffer of data to write to the wire transport.</param>
        /// <param name="offset">The location within the buffer to begin reading.</param>
        /// <param name="count">The amount of bytes to write to the wire transport.</param>
        /// <returns>The number of bytes written to the wire transport.</returns>
        Task<int> SendAsync(byte[] buffer, int offset, int count);
    }
}
