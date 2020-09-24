// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using Microsoft.Bot.Streaming.Payloads;
using Microsoft.Bot.Streaming.Transport;

namespace Microsoft.Bot.Streaming.PayloadTransport
{
    /// <summary>
    /// PayloadReceivers subscribe to incoming streams and manage the consumption of raw data as it comes in.
    /// </summary>
    public interface IPayloadReceiver
    {
        /// <summary>
        /// Emitted when the PayloadReceiver becomes disconneced from the <see cref="ITransportReceiver"/>.
        /// </summary>
        event DisconnectedEventHandler Disconnected;

        /// <summary>
        /// Gets a value indicating whether the PayloadReceiver is currently connected to an <see cref="ITransportReceiver"/>.
        /// </summary>
        /// <value>
        /// The value indicating if the PayloadReceiver is currently connected to an <see cref="ITransportReceiver"/>.
        /// </value>
        bool IsConnected { get; }

        /// <summary>
        /// Connects the PayloadReceiver to the passed in <see cref="ITransportReceiver"/>.
        /// </summary>
        /// <param name="receiver">The <see cref="ITransportReceiver"/> to connect this PayloadReceiver to.</param>
        void Connect(ITransportReceiver receiver);

        /// <summary>
        /// Sets the behaviors used to attach to a specified <see cref="Stream"/> and on receiving data on said Stream.
        /// </summary>
        /// <param name="getStream">The function executed to attach to the specified <see cref="Stream"/>.</param>
        /// <param name="receiveAction">The function to execute when new data is received from the attached <see cref="Stream"/>.</param>
        void Subscribe(Func<Header, Stream> getStream, Action<Header, Stream, int> receiveAction);

        /// <summary>
        /// Disconnects the PayloadReceiver from its <see cref="ITransportReceiver"/>.
        /// </summary>
        /// <param name="e">Optional arguments to proprogate during disconnection.</param>
        void Disconnect(DisconnectedEventArgs e = null);
    }
}
