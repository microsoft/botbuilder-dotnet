// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Streaming.PayloadTransport
{
    public class TransportDisconnectedException : Exception
    {
        public TransportDisconnectedException()
            : base()
        {
        }

        public TransportDisconnectedException(string message)
            : base(message)
        {
        }

        public TransportDisconnectedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public string Reason => Message;
    }
}
