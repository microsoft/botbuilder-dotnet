﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Streaming.Payloads;

namespace Microsoft.Bot.Connector.Streaming.Application
{
    /// <summary>
    /// Delegate used to setup actions to be taken when disconnection events are triggered.
    /// </summary>
    /// <param name="sender">The source of the disconnection event.</param>
    /// <param name="e">The arguments specified by the disconnection event.</param>
    public delegate void DisconnectedEventHandler(object sender, DisconnectedEventArgs e);

    /// <summary>
    /// Implemented by clients compatible with the Bot Framework Protocol 3 with Streaming Extensions.
    /// </summary>
    public interface IStreamingTransportClient : IDisposable
    {
        /// <summary>
        /// An event used to signal when the underlying connection has disconnected.
        /// </summary>
        event DisconnectedEventHandler Disconnected;

        /// <summary>
        /// Gets a value indicating whether this client is currently connected.
        /// </summary>
        /// <value>
        /// True if this client is currently connected, otherwise false.
        /// </value>
        bool IsConnected { get; }

        /// <summary>
        /// The task used to establish a connection for this client.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task ConnectAsync();

        /// <summary>
        /// Establish a connection passing along additional headers.
        /// </summary>
        /// <param name="requestHeaders">Dictionary of header name and header value to be passed during connection. Generally, you will need channelID and Authorization.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task ConnectAsync(IDictionary<string, string> requestHeaders);

        /// <summary>
        /// Task used to send data over this client connection.
        /// </summary>
        /// <param name="message">The <see cref="StreamingRequest"/> to send.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> used to signal this operation should be cancelled.</param>
        /// <returns>A <see cref="Task"/> of type <see cref="ReceiveResponse"/> handling the send operation.</returns>
        Task<ReceiveResponse> SendAsync(StreamingRequest message, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Method used to disconnect this client.
        /// </summary>
        void Disconnect();
    }

    /// <summary>
    /// Arguments to be included when disconnection events are fired.
    /// </summary>
    public class DisconnectedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets an empty set of arguments.
        /// </summary>
        /// <value>
        /// A new, empty, set of <see cref="DisconnectedEventArgs"/>.
        /// </value>
        public static new DisconnectedEventArgs Empty { get; set; } = new DisconnectedEventArgs();

        /// <summary>
        /// Gets or sets the reason field of the arguments.
        /// </summary>
        /// <value>
        /// The reason the disconnection event fired, in plain text.
        /// </value>
        public string Reason { get; set; }
    }
}
