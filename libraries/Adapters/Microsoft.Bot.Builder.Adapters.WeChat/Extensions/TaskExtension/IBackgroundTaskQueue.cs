using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Microsoft.Bot.Builder.Adapters.WeChat.TaskExtensions
{
    public interface IBackgroundTaskQueue
    {
        void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem);

        Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken token);
    }
}