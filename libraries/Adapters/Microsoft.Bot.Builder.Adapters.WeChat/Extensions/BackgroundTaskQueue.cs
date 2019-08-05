// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Extensions
{
    public class BackgroundTaskQueue : IBackgroundTaskQueue, IDisposable
    {
        public static readonly BackgroundTaskQueue Instance = new BackgroundTaskQueue();

        private readonly ConcurrentQueue<Func<CancellationToken, Task>> _workItems = new ConcurrentQueue<Func<CancellationToken, Task>>();
        private SemaphoreSlim _signal = new SemaphoreSlim(0);

        /// <summary>
        /// Queue a Task to background task queue.
        /// </summary>
        /// <param name="workItem">The work func need to be queued.</param>
        public void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            _workItems.Enqueue(workItem);
            _signal.Release();
        }

        /// <summary>
        /// Dequeue a Task in background task queue.
        /// </summary>
        /// <param name="token">The work func need to be queued.</param>
        /// <returns>A <see cref="Task"/> representing the dequeue operation.</returns>
        public async Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken token)
        {
            await _signal.WaitAsync(token).ConfigureAwait(false);

            _workItems.TryDequeue(out var workItem);

            return workItem;
        }

        public void Dispose()
        {
            _signal.Dispose();
        }
    }
}
