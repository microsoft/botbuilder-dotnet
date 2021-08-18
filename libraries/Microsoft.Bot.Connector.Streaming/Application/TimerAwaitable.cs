// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Connector.Streaming.Application
{
    // Reusing internal awaitable timer from https://github.com/dotnet/aspnetcore/blob/main/src/SignalR/common/Shared/TimerAwaitable.cs
    internal class TimerAwaitable : IDisposable, INotifyCompletion
    {
        private static readonly Action _callbackCompleted = () => { };

        private Timer _timer;
        private Action _callback;

        private readonly TimeSpan _period;

        private readonly TimeSpan _dueTime;
        private readonly object _lockObj = new object();
        private bool _disposed;
        private bool _running = true;

        public TimerAwaitable(TimeSpan dueTime, TimeSpan period)
        {
            _dueTime = dueTime;
            _period = period;
        }

        public bool IsCompleted => ReferenceEquals(_callback, _callbackCompleted);

        public void Start()
        {
            if (_timer == null)
            {
                lock (_lockObj)
                {
                    if (_disposed)
                    {
                        return;
                    }

                    if (_timer == null)
                    {
                        // This fixes the cycle by using a WeakReference to the state object. The object graph now looks like this:
                        // Timer -> TimerHolder -> TimerQueueTimer -> WeakReference<TimerAwaitable> -> Timer -> ...
                        // If TimerAwaitable falls out of scope, the timer should be released.
                        _timer = NonCapturingTimer.Create(
                            state =>
                            {
                                var weakRef = (WeakReference<TimerAwaitable>)state!;
                                if (weakRef.TryGetTarget(out var thisRef))
                                {
                                    thisRef.Tick();
                                }
                            },
                            state: new WeakReference<TimerAwaitable>(this),
                            dueTime: _dueTime,
                            period: _period);
                    }
                }
            }
        }

        public TimerAwaitable GetAwaiter() => this;

        public bool GetResult()
        {
            _callback = null;

            return _running;
        }

        public void OnCompleted(Action continuation)
        {
            if (ReferenceEquals(_callback, _callbackCompleted) ||
                ReferenceEquals(Interlocked.CompareExchange(ref _callback, continuation, null), _callbackCompleted))
            {
                _ = Task.Run(continuation);
            }
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            OnCompleted(continuation);
        }

        public void Stop()
        {
            lock (_lockObj)
            {
                // Stop should be used to trigger the call to end the loop which disposes
                if (_disposed)
                {
                    throw new ObjectDisposedException(GetType().FullName);
                }

                _running = false;
            }

            // Call tick here to make sure that we yield the callback,
            // if it's currently waiting, we don't need to wait for the next period
            Tick();
        }

        void IDisposable.Dispose()
        {
            lock (_lockObj)
            {
                _disposed = true;

                _timer?.Dispose();

                _timer = null;
            }
        }

        private void Tick()
        {
            var continuation = Interlocked.Exchange(ref _callback, _callbackCompleted);
            continuation?.Invoke();
        }

        // A convenience API for interacting with System.Threading.Timer in a way
        // that doesn't capture the ExecutionContext. We should be using this (or equivalent)
        // everywhere we use timers to avoid rooting any values stored in asynclocals.
        private static class NonCapturingTimer
        {
            public static Timer Create(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
            {
                if (callback == null)
                {
                    throw new ArgumentNullException(nameof(callback));
                }

                // Don't capture the current ExecutionContext and its AsyncLocals onto the timer
                bool restoreFlow = false;
                try
                {
                    if (!ExecutionContext.IsFlowSuppressed())
                    {
                        ExecutionContext.SuppressFlow();
                        restoreFlow = true;
                    }

                    return new Timer(callback, state, dueTime, period);
                }
                finally
                {
                    // Restore the current ExecutionContext
                    if (restoreFlow)
                    {
                        ExecutionContext.RestoreFlow();
                    }
                }
            }
        }
    }
}
