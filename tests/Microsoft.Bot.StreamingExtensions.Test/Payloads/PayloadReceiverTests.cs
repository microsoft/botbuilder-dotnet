// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.StreamingExtensions.PayloadTransport;
using Microsoft.Bot.StreamingExtensions.UnitTests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.StreamingExtensions.UnitTests.Payloads
{
    [TestClass]
    public class PayloadReceiverTests
    {
        [TestMethod]
        public async Task PayloadReceiver_ReceivePacketsAsync_ReceiveShortHeader_Throws()
        {
            var disconnectEvent = new TaskCompletionSource<string>();

            var buffer = new byte[20];
            var random = new Random();
            random.NextBytes(buffer);

            var transport = new MockTransportReceiver(buffer);

            var receiver = new PayloadReceiver();
            receiver.Disconnected += (sender, e) =>
            {
                Assert.AreEqual("Stream closed while reading header bytes", e.Reason);
                disconnectEvent.SetResult("done");
            };

            receiver.Connect(transport);

            var result = await disconnectEvent.Task;

            Assert.AreEqual("done", result);
        }
    }
}
