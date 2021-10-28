// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Streaming.PayloadTransport;
using Microsoft.Bot.Streaming.UnitTests.Mocks;
using Xunit;

namespace Microsoft.Bot.Streaming.UnitTests.Payloads
{
    public class PayloadReceiverTests
    {
        [Fact]
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
                Assert.Equal("Stream closed while reading header bytes", e.Reason);
                disconnectEvent.SetResult("done");
            };

            receiver.Connect(transport);

            var result = await disconnectEvent.Task;

            Assert.Equal("done", result);
        }

        [Fact]
        public void PayloadReceiver_Connect_ShouldFail_In_Windows()
        {
            var buffer = new byte[20];

            var transport = new MockTransportReceiver(buffer);
            var receiver = new PayloadReceiver();

            // First connection succeeds.
            receiver.Connect(transport);

            bool onBuildAgent = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AGENT_OS"));
            bool winNtOnAgent = onBuildAgent &&
                Environment.GetEnvironmentVariable("AGENT_OS").Equals("Windows_NT");
            bool netCore46 = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription.Contains("Core 4.6");

            if (!onBuildAgent || winNtOnAgent || netCore46)
            {
                Assert.True(false, $"Top: onBuildAgent={onBuildAgent}; winNtOnAgent={winNtOnAgent}; netCore46={netCore46}");
                // In 1) Windows and 2) MacLinux .NET Core 4.6 the second connection throws an exception.
                Assert.Throws<InvalidOperationException>(() => receiver.Connect(transport));
            }
            else
            {
                Assert.True(false, $"Bottom: onBuildAgent={onBuildAgent}; winNtOnAgent={winNtOnAgent}; netCore46={netCore46}");
                // In MacLinux .NET Core 3.1 the second connection does not throw.
                receiver.Connect(transport);
            }
        }
    }
}
