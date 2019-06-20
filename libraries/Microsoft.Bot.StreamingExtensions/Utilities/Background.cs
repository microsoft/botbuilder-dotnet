using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Protocol.Utilities
{
    public static class Background
    {
        private static async Task TrackAsRequest(Func<Task> task, IDictionary<string, object> properties)
        {
            try
            {
                await task().ConfigureAwait(false);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Register background task with ASP.Net hosting environment and trace exceptions
        /// Falls back to Thread pool if not running under ASP.Net
        /// </summary>
        /// <param name="task">background task to execute</param>
        /// <param name="properties">name value pairs to trace if an exception is thrown</param>
        public static void Run(Func<Task> task, IDictionary<string, object> properties = null)
        {
            Run((ct) => task(), properties);
        }

        /// <summary>
        /// Register background task with ASP.Net hosting environment and trace exceptions
        /// Falls back to Thread pool if not running under ASP.Net
        /// </summary>
        /// <param name="task">background task to execute</param>
        /// <param name="properties">name value pairs to trace if an exception is thrown</param>
        public static void Run(Func<CancellationToken, Task> task, IDictionary<string, object> properties = null)
        {
            Task.Run(() => TrackAsRequest(() => task(CancellationToken.None), properties));
        }
        
        /// <summary>
        /// Register periodic background task with ASP.Net hosting environment and trace exceptions.
        /// </summary>
        /// <param name="task">background task to execute</param>
        /// <param name="spanDelay">the initial delay</param>
        /// <param name="eventName">the event name to log individual execution failures</param>
        public static void RunForever(Func<CancellationToken, TimeSpan> task, TimeSpan spanDelay, string eventName)
        {
            RunForever(token => Task.FromResult(task(token)), spanDelay, eventName);
        }

        /// <summary>
        /// Register periodic background task with ASP.Net hosting environment and trace exceptions.
        /// </summary>
        /// <param name="task">background task to execute</param>
        /// <param name="spanDelay">the initial delay</param>
        /// <param name="eventName">the event name to log individual execution failures</param>
        public static void RunForever(Func<CancellationToken, Task<TimeSpan>> task, TimeSpan spanDelay, string eventName)
        {
            Background.Run(async token =>
            {
                try
                {
                    await Task.Delay(spanDelay, token).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested)
                {
                    // swallow these so we don't log exceptions on normal shutdown
                }

                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        spanDelay = await task(token).ConfigureAwait(false);
                    }
                    catch (Exception)
                    {
                    }

                    try
                    {
                        await Task.Delay(spanDelay, token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) when (token.IsCancellationRequested)
                    {
                        // swallow these so we don't log exceptions on normal shutdown
                    }
                }
            });
        }
    }
}
