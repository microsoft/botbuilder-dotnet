// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Streaming.Application;
using Microsoft.Bot.Connector.Streaming.Tests.Tools;
using Xunit;

namespace Microsoft.Bot.Connector.Streaming.Tests.Application
{
    public class TimerAwaitableTests
    {
        [Fact]
        public async Task FinalizerRunsIfTimerAwaitableReferencesObject()
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            UseTimerAwaitableAndUnref(tcs);

            GC.Collect();
            GC.WaitForPendingFinalizers();

            // Make sure the finalizer runs
            await tcs.Task.TimeoutAfter(TimeSpan.FromSeconds(30));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void UseTimerAwaitableAndUnref(TaskCompletionSource<bool> tcs)
        {
            _ = new ObjectWithTimerAwaitable(tcs).Start();
        }
    }
}
