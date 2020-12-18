// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Streaming.Utilities
{
    internal static class Background
    {
        /// <summary>
        /// Register background task with ASP.Net hosting environment and trace exceptions
        /// Falls back to Thread pool if not running under ASP.Net.
        /// </summary>
        /// <param name="task">Background task to execute.</param>
        /// <param name="properties">Name value pairs to trace if an exception is thrown.</param>
        public static void Run(Func<Task> task, IDictionary<string, object> properties = null)
        {
            Run((ct) => task(), properties);
        }

        /// <summary>
        /// Register background task with ASP.Net hosting environment and trace exceptions
        /// Falls back to Thread pool if not running under ASP.Net.
        /// </summary>
        /// <param name="task">background task to execute.</param>
        /// <param name="properties">name value pairs to trace if an exception is thrown.</param>
#pragma warning disable CA1801 // Review unused parameters
        public static void Run(Func<CancellationToken, Task> task, IDictionary<string, object> properties = null)
#pragma warning restore CA1801 // Review unused parameters
        {
#pragma warning disable VSTHRD110 // Observe result of async calls
            Task.Run(() => TrackAsRequestAsync(() => task(CancellationToken.None)));
#pragma warning restore VSTHRD110 // Observe result of async calls
        }

        /// <summary>
        /// Register periodic background task with ASP.Net hosting environment and trace exceptions.
        /// </summary>
        /// <param name="task">background task to execute.</param>
        /// <param name="spanDelay">the initial delay.</param>
        /// <param name="eventName">the event name to log individual execution failures.</param>
        public static void RunForever(Func<CancellationToken, TimeSpan> task, TimeSpan spanDelay, string eventName)
        {
            RunForever(token => Task.FromResult(task(token)), spanDelay, eventName);
        }

        /// <summary>
        /// Register periodic background task with ASP.Net hosting environment and trace exceptions.
        /// </summary>
        /// <param name="task">Background task to execute.</param>
        /// <param name="spanDelay">The initial delay.</param>
        /// <param name="eventName">The event name to log individual execution failures.</param>
#pragma warning disable CA1801 // Review unused parameters (we can't change this without breaking binary compat)
        public static void RunForever(Func<CancellationToken, Task<TimeSpan>> task, TimeSpan spanDelay, string eventName)
#pragma warning restore CA1801 // Review unused parameters
        {
            Run(async token =>
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
#pragma warning disable CA1031 // Do not catch general exception types
                    catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
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

        private static async Task TrackAsRequestAsync(Func<Task> task)
        {
            try
            {
                await task().ConfigureAwait(false);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
            {
            }
        }
    }
}
