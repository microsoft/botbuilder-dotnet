// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Xunit;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.Tests
{
    public class BackgroundTaskServiceTests
    {
        [Fact]
        public async Task ValidateTaskAutoRemoval()
        {
            var bts = new BackgroundTaskService();
            bts.AddTask(Task.Delay(1000));
            bts.AddTask(Task.Delay(1000));
            bts.AddTask(Task.Delay(1000));
            bts.AddTask(Task.Delay(1000));
            await Task.Delay(1);
            Assert.Equal(4, bts.Pending);
            await Task.Delay(1000);
            Assert.Equal(0, bts.Pending);
        }

        [Fact]
        public async Task ValidateStopNone()
        {
            var bts = new BackgroundTaskService();
            Assert.Equal(0, bts.Pending);
            await bts.StopAsync(CancellationToken.None);
            Assert.Equal(0, bts.Pending);
        }

        [Fact]
        public async Task ValidateStop_WithTasks()
        {
            var bts = new BackgroundTaskService();
            bts.AddTask(Task.Delay(1000));
            bts.AddTask(Task.Delay(1000));
            bts.AddTask(Task.Delay(1000));
            bts.AddTask(Task.Delay(1000));
            Assert.Equal(4, bts.Pending);
            await bts.StopAsync(CancellationToken.None);
            Assert.Equal(0, bts.Pending);
        }
    }
}
