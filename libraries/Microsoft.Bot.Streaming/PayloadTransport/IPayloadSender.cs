// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Bot.Streaming.Payloads;
using Microsoft.Bot.Streaming.Transport;

namespace Microsoft.Bot.Streaming.PayloadTransport
{
    /// <summary>
    /// Each PayloadSender manages writing raw data to an outgoing <see cref="ITransportSender"/>.
    /// </summary>
    public interface IPayloadSender
    {
        /// <summary>
        /// Emitted when the PayloadSender becomes disconneced from the <see cref="ITransportSender"/>.
        /// </summary>
        event DisconnectedEventHandler Disconnected;

        /// <summary>
        /// Gets a value indicating whether the PayloadSender is currently connected to an <see cref="ITransportSender"/>.
        /// </summary>
        /// <value>
        /// The value indicating if the PayloadSender is currently connected to an <see cref="ITransportSender"/>.
        /// </value>
        bool IsConnected { get; }

        /// <summary>
        /// Connects the PayloadSender to the passed in <see cref="ITransportSender"/>.
        /// </summary>
        /// <param name="sender">The <see cref="ITransportSender"/> to connect this PayloadSender to.</param>
        void Connect(ITransportSender sender);

        /// <summary>
        /// Begins the process of writing the given payload to the outgoing <see cref="Stream"/> and sets the callback to trigger when complete.
        /// </summary>
        /// <param name="header">The <see cref="Header"/> to write to the outgoing <see cref="PayloadStream"/>.</param>
        /// <param name="payload">The <see cref="Stream"/> containing the data to write to the outgoing <see cref="PayloadStream"/>.</param>
        /// <param name="isLengthKnown">True if the stream length is known, otherwise false.</param>
        /// <param name="sentCallback">The function to trigger once the send operation is complete.</param>
        void SendPayload(Header header, Stream payload, bool isLengthKnown, Func<Header, Task> sentCallback);

        /// <summary>
        /// Disconnects the PayloadSender from its <see cref="ITransportSender"/>.
        /// </summary>
        /// <param name="e">Optional arguments to proprogate during disconnection.</param>
        void Disconnect(DisconnectedEventArgs e = null);
    }
}
