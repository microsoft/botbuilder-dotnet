// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters.WeChat.Extensions;
using Xunit;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Tests
{
    public class BackgroundTaskTest
    {
        [Fact]
        public async Task BackgroundTaskQueueTest()
        {
            var queue = new BackgroundTaskQueue();
            var hostService = new QueuedHostedService(queue);
            var result = string.Empty;
            queue.QueueBackgroundWorkItem(ct =>
            {
                result = "executed";
                return Task.CompletedTask;
            });
            await hostService.StartAsync(default(CancellationToken));
            queue.Dispose();
            Assert.Equal("executed", result);
        }
    }
}
