using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.StreamingExtensions.Transport
{
    public interface IStreamingTransportClient : IDisposable
    {
        event DisconnectedEventHandler Disconnected;

        /// <summary>
        /// Establish a connection.
        /// </summary>
        Task ConnectAsync();

        /// <summary>
        /// Establish a connection passing along additional headers.
        /// </summary>
        /// <param name="requestHeaders">Dictionary of header name and header value to be passed during connection. Generally, you will need channelID and Authorization.</param>
        Task ConnectAsync(IDictionary<string, string> requestHeaders);

        Task<ReceiveResponse> SendAsync(Request message, CancellationToken cancellationToken = default(CancellationToken));

        bool IsConnected { get; }

        void Disconnect();
    }
}
