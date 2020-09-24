// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Streaming.Utilities;

namespace Microsoft.Bot.Streaming.PayloadTransport
{
    internal class SendQueue<T> : IDisposable
    {
        private readonly Func<T, Task> _action;

        // _queue and _semaphore are interlocked by _lock
        private readonly Queue<object> _queue = new Queue<object>();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0, 1);
        private readonly EventWaitHandle _completionEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
        private readonly object _lock = new object();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly int _timeoutSeconds;

        // To detect redundant calls to dispose
        private bool _disposed;

        public SendQueue(Func<T, Task> action, int timeoutSeconds = 30)
        {
            _action = action;
            _timeoutSeconds = timeoutSeconds;
            Background.Run(ProcessAsync);
        }

        public void Post(T item)
        {
            PostInternal(item);
        }

        /// <summary>
        /// Disposes the object and releases any related objects owned by the class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes objected used by the class.
        /// </summary>
        /// <param name="disposing">A Boolean that indicates whether the method call comes from a Dispose method (its value is true) or from a finalizer (its value is false).</param>
        /// <remarks>
        /// The disposing parameter should be false when called from a finalizer, and true when called from the IDisposable.Dispose method.
        /// In other words, it is true when deterministically called and false when non-deterministically called.
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // Dispose managed objects owned by the class here.
                _cts?.Cancel();

                // Wait for thread to drain
                if (_completionEvent != null && !_completionEvent.WaitOne(_timeoutSeconds * 1000))
                {
                    // TODO:  Flow ILogger to this layer.
                    // AppInsights.TrackEvent("SendQueue.Dispose: _completionEvent timeout!!!", "Timeout Seconds".PairWith(timeoutSeconds));
                }

                _completionEvent?.Dispose();
                _semaphore?.Dispose();
                _cts?.Dispose();
            }

            _disposed = true;
        }

        private void PostInternal(object item)
        {
            // Bail out if disposed
            if (!_cts.Token.IsCancellationRequested)
            {
                // Acquire _lock so we can queue and set the event
                lock (_lock)
                {
                    _queue.Enqueue(item);
                    if (_semaphore.CurrentCount == 0)
                    {
                        _semaphore.Release();
                    }
                }
            }
        }

        private async Task ProcessAsync()
        {
            try
            {
                while (true)
                {
                    try
                    {
                        await _semaphore.WaitAsync(_cts.Token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }

                    while (true)
                    {
                        // Bail out if we were disposed during this loop
                        if (_cts.IsCancellationRequested)
                        {
                            return;
                        }

                        // Acquire _lock so we can dequeue and maybe reset the event
                        object obj;
                        lock (_lock)
                        {
                            // No more work to do, break back to main loop
                            if (_queue.Count == 0)
                            {
                                break;
                            }

                            obj = _queue.Dequeue();
                        }

                        // Queue item
                        try
                        {
                            // Invoke operation
                            await _action((T)obj).ConfigureAwait(false);
                        }
#pragma warning disable CA1031 // Do not catch general exception types

                        // We don't want to stop this loop because of any kind of exception for now
                        catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
                        {
                            // TODO: Flow ILogger to this layer and log these exceptions.
                            // AppInsights.TrackException(e);
                        }
                    }
                }
            }
            finally
            {
                _completionEvent.Set();
            }
        }
    }
}
