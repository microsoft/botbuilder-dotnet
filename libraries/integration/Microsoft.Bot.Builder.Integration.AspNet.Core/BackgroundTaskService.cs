using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    /// <summary>
    /// Class which tracks running tasks so that when service shuts down it waits for them to finish before shutting down.
    /// </summary>
    internal class BackgroundTaskService : IHostedService
    {
        private long _taskCounter = 1;
        private Dictionary<long, Task> _runningTasks = new Dictionary<long, Task>();

        /// <summary>
        /// Called when service starts.
        /// </summary>
        /// <param name="cancellationToken">cancellation token.</param>
        /// <returns>task.</returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called when service is stopping.
        /// </summary>
        /// <param name="cancellationToken">cancellation token.</param>
        /// <returns>task.</returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            // wait for all running tasks
            return Task.WhenAll(_runningTasks.Values);
        }

        public void AddTask(Task task)
        {
            var id = Interlocked.Increment(ref _taskCounter);
            task = task.ContinueWith(
                t =>
                {
                    _runningTasks.Remove(id);
                    if (t.IsFaulted)
                    {
                        Trace.TraceError($"Task failed: {t.Exception.Message}", t.Exception);
                    }
                }, TaskScheduler.Default);

            _runningTasks[id] = task;
        }
    }
}
