// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Rest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

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
