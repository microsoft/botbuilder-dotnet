// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Base
{
#pragma warning disable CA1815 // Override equals and operator equals on value types (we don't seem to need this in code, excluding for now)
    internal struct Releaser : IDisposable
#pragma warning restore CA1815 // Override equals and operator equals on value types
    {
        public Releaser(SemaphoreSlim semaphore)
        {
            Semaphore = semaphore ?? throw new ArgumentNullException(nameof(semaphore));
        }

        public SemaphoreSlim Semaphore { get; }

        public void Dispose()
        {
            Semaphore.Release();
        }
    }
}
