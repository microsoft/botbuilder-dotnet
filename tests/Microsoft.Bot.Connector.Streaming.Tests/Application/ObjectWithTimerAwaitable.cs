// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Streaming.Application;

namespace Microsoft.Bot.Connector.Streaming.Tests.Application
{
    // This object holds onto a TimerAwaitable referencing the callback (the async continuation is the callback)
    // it also has a finalizer that triggers a tcs so callers can be notified when this object is being cleaned up.
    public class ObjectWithTimerAwaitable
    {
        private readonly TimerAwaitable _timer;
        private readonly TaskCompletionSource<bool> _tcs;

        public ObjectWithTimerAwaitable(TaskCompletionSource<bool> tcs)
        {
            _tcs = tcs;
            _timer = new TimerAwaitable(TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(1));
            _timer.Start();
        }

        ~ObjectWithTimerAwaitable()
        {
            _tcs.TrySetResult(true);
        }

        public async Task Start()
        {
            using (_timer)
            {
                while (await _timer)
                {
                }
            }
        }
    }
}
