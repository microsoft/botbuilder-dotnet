// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Buffers;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Connector.Streaming.Transport
{
    internal static class WebSocketExtensions
    {
        public static ValueTask SendAsync(this WebSocket webSocket, ReadOnlySequence<byte> buffer, WebSocketMessageType webSocketMessageType, CancellationToken cancellationToken = default)
        {
            if (buffer.IsSingleSegment)
            {
                var isArray = MemoryMarshal.TryGetArray(buffer.First, out var segment);
                return new ValueTask(webSocket.SendAsync(segment, webSocketMessageType, endOfMessage: true, cancellationToken));
            }
            else
            {
                return SendMultiSegmentAsync(webSocket, buffer, webSocketMessageType, cancellationToken);
            }
        }

        private static async ValueTask SendMultiSegmentAsync(WebSocket webSocket, ReadOnlySequence<byte> buffer, WebSocketMessageType webSocketMessageType, CancellationToken cancellationToken = default)
        {
            var position = buffer.Start;

            // Get a segment before the loop so we can be one segment behind while writing
            // This allows us to do a non-zero byte write for the endOfMessage = true send
            buffer.TryGet(ref position, out var prevSegment);
            while (buffer.TryGet(ref position, out var segment))
            {
                var isArray = MemoryMarshal.TryGetArray(prevSegment, out var arraySegment);
                await webSocket.SendAsync(arraySegment, webSocketMessageType, endOfMessage: false, cancellationToken).ConfigureAwait(false);
                prevSegment = segment;
            }

            // End of message frame
            if (MemoryMarshal.TryGetArray(prevSegment, out var arraySegmentEnd))
            {
                await webSocket.SendAsync(arraySegmentEnd, webSocketMessageType, endOfMessage: true, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
