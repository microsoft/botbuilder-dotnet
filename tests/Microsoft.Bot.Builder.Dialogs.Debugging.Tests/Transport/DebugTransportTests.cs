// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Debugging.Transport;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Tests.Transport
{
    public sealed class DebugTransportTests
    {
        [Fact]
        public void DebugTransport_Constructor()
        {
            var transport = new DebugTransport(3980, null);

            Assert.NotNull(transport);

            transport.Dispose();
        }

        [Fact]
        public void DebugTransport_Dispose()
        {
            var transport = new DebugTransport(3981, null);

            transport.Dispose();

            Assert.Throws<ObjectDisposedException>(() =>
            {
                transport.Dispose();
            });
        }

        [Fact]
        public void DebugTransport_Accept()
        {
            IDebugTransport transport = new DebugTransport(3982, null);

            transport.Accept = AcceptAsync;

            Assert.NotNull(transport.Accept);
        }

        [Fact]
        public async Task DebugTransport_ReadAsync_Throws()
        {
            IDebugTransport transport = new DebugTransport(3983, null);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await transport.ReadAsync(default);
            });
        }

        private static async Task AcceptAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
