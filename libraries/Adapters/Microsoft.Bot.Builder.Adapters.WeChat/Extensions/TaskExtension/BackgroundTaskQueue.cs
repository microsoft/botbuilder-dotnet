using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Microsoft.Bot.Builder.Adapters.WeChat.TaskExtensions
{
    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        public static readonly BackgroundTaskQueue Instance = new BackgroundTaskQueue();

        private ConcurrentQueue<Func<CancellationToken, Task>> _workItems = new ConcurrentQueue<Func<CancellationToken, Task>>();
        private SemaphoreSlim _signal = new SemaphoreSlim(0);

        public void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            this._workItems.Enqueue(workItem);
            this._signal.Release();
        }

        public async Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken token)
        {
            await this._signal.WaitAsync(token).ConfigureAwait(false);

            this._workItems.TryDequeue(out var workItem);

            return workItem;
        }
    }
}