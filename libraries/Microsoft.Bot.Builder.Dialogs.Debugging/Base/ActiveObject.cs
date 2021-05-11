using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Base
{
    /// <summary>
    /// Implementation of the active object pattern.
    /// https://en.wikipedia.org/wiki/Active_object
    /// Invokes a task with a cooperative cancellation token that runs until disposable.
    /// Most useful for running a task during a using-block scope.
    /// </summary>
    internal sealed class ActiveObject : IDisposable
    {
        private readonly Task _task;
        private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        public ActiveObject(Func<CancellationToken, Task> invokeAsync)
        {
            if (invokeAsync == null)
            {
                throw new ArgumentNullException(nameof(invokeAsync));
            }

            _task = invokeAsync(_cancellationToken.Token);
        }

        public void Dispose()
        {
            DisposeAsync().GetAwaiter().GetResult();
        }

        public async Task DisposeAsync()
        {
            // initiate the cancellation
            _cancellationToken.Cancel();

            // dispose all owned objects
            using (_cancellationToken)
#pragma warning disable VSTHRD107 // Await Task within using expression
            using (_task)
#pragma warning restore VSTHRD107 // Await Task within using expression
            {
                try
                {
                    // wait for the completion of the task
                    await _task.ConfigureAwait(false);
                }
                catch (OperationCanceledException error) when (error.CancellationToken == _cancellationToken.Token)
                {
                    // swallow exceptions expected from cancellation
                }
            }
        }
    }
}
