using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Connector.Streaming
{ 
    internal static class TaskExtensions
    {
        private static readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(30);

        public static async Task<T> DefaultTimeOutAsync<T>(this Task<T> task)
        {
            return await task.TimeoutAfterAsync<T>(_defaultTimeout).ConfigureAwait(false);
        }

        public static async Task<T> TimeoutAfterAsync<T>(this Task<T> task, TimeSpan timeout)
        {
            // Don't create a timer if the task is already completed
            // or the debugger is attached
            if (task.IsCompleted || Debugger.IsAttached)
            {
                return await task.ConfigureAwait(false);
            }

            using (var cts = new CancellationTokenSource())
            {
                if (task == await Task.WhenAny(task, Task.Delay(timeout, cts.Token)).ConfigureAwait(false))
                {
                    cts.Cancel();
                    return await task.ConfigureAwait(false);
                }
                else
                {
                    throw new TimeoutException();
                }
            }
        }

        public static async Task DefaultTimeOutAsync(this Task task)
        {
            await task.TimeoutAfterAsync(_defaultTimeout).ConfigureAwait(false);
        }

        public static async Task TimeoutAfterAsync(this Task task, TimeSpan timeout)
        {
            // Don't create a timer if the task is already completed
            // or the debugger is attached
            if (task.IsCompleted || Debugger.IsAttached)
            {
                await task.ConfigureAwait(false);
                return;
            }

            using (var cts = new CancellationTokenSource())
            {
                if (task == await Task.WhenAny(task, Task.Delay(timeout, cts.Token)).ConfigureAwait(false))
                {
                    cts.Cancel();
                    await task.ConfigureAwait(false);
                }
                else
                {
                    throw new TimeoutException();
                }
            }
        }
    }
}
