// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Microsoft.Bot.StreamingExtensions.UnitTests.Mocks")]

namespace Microsoft.Bot.StreamingExtensions.Transport
{
#if DEBUG
    public
#else
    internal
#endif
    interface ITransport : IDisposable
    {
        bool IsConnected { get; }

        void Close();
    }
}
