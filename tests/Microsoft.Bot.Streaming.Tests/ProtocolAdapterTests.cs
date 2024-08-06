// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Streaming.Payloads;
using Microsoft.Bot.Streaming.PayloadTransport;
using Moq;
using Xunit;

namespace Microsoft.Bot.Streaming.UnitTests
{
    public class ProtocolAdapterTests
    {
        [Fact]

        public async Task SendRequestAsync_With_No_StreamingRequest_Should_Fail()
        {
            var requestHandler = new Mock<RequestHandler>();
            var requestManager = new Mock<RequestManager>();
            var payloadSender = new Mock<PayloadSender>();
            var payloadReceiver = new Mock<PayloadReceiver>();
            var protocolAdapter = new ProtocolAdapter(requestHandler.Object, requestManager.Object, payloadSender.Object, payloadReceiver.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(() => protocolAdapter.SendRequestAsync(null));
        }
    }
}
