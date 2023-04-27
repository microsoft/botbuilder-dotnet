// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Streaming.Tests.Utilities;
using Microsoft.Bot.Streaming.Transport.WebSockets;
using Xunit;

namespace Microsoft.Bot.Streaming.UnitTests
{
    public class WebSocketTransportTests
    {
        [Fact]
        public void WebSocketTransport_Connects()
        {
            var sock = new FauxSock();
            sock.RealState = WebSocketState.Open;
            var transport = new WebSocketTransport(sock);

            Assert.True(transport.IsConnected);

            transport.Close();
            transport.Dispose();
        }

        [Fact]
        public void WebSocketTransport_SetsState()
        {
            var sock = new FauxSock();
            sock.RealState = WebSocketState.Open;
            var transport = new WebSocketTransport(sock);

            transport.Close();
            transport.Dispose();

            Assert.Equal(WebSocketState.Closed, sock.RealState);
        }

        [Fact]
        public async Task WebSocketTransport_CanSend()
        {
            // Arrange
            var sock = new FauxSock();
            sock.RealState = WebSocketState.Open;
            var transport = new WebSocketTransport(sock);
            var messageText = "This is a message.";
            byte[] message = Encoding.ASCII.GetBytes(messageText);

            // Act
            await transport.SendAsync(message, 0, message.Length);

            // Assert
            Assert.Equal(messageText, Encoding.UTF8.GetString(sock.SentArray));
        }

        [Fact]
        public async Task WebSocketTransport_CanReceive()
        {
            // Arrange
            var sock = new FauxSock();
            sock.RealState = WebSocketState.Open;
            var transport = new WebSocketTransport(sock);
            byte[] message = Encoding.ASCII.GetBytes("This is a message.");

            // Act
            var received = await transport.ReceiveAsync(message, 0, message.Length);

            // Assert
            Assert.Equal(message.Length, received);
        }
    }
}
