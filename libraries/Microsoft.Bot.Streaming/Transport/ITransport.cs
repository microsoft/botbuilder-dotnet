// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Streaming.Transport
{
    /// <summary>
    /// A transport format used when writing data to the wire.
    /// </summary>
    public interface ITransport : IDisposable
    {
        /// <summary>
        ///  Gets a value indicating whether the transport is connected.
        /// </summary>
        /// <value>
        /// A value indicating whether the transport is connected.
        /// </value>
        bool IsConnected { get; }

        /// <summary>
        /// Closes the transport.
        /// </summary>
        void Close();
    }
}
