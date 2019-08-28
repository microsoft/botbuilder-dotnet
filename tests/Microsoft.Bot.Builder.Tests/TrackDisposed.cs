// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Builder.Tests
{
    /// <summary>
    /// Vanilla <see cref="IDisposable"/> tracks if Dispose has been called.
    /// </summary>
    /// <remarks>Moq failed to create this properly.  Boo moq!.</remarks>
    public class TrackDisposed : IDisposable
    {
        public bool Disposed { get; private set; } = false;

        public void Dispose()
        {
            Disposed = true;
        }
    }
}
