// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
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
        public static void Run(Func<Task> task)
        {
            Run((ct) => task());
        }

        /// <summary>
        /// Register background task with ASP.Net hosting environment and trace exceptions
        /// Falls back to Thread pool if not running under ASP.Net.
        /// </summary>
        /// <param name="task">background task to execute.</param>
        public static void Run(Func<CancellationToken, Task> task)
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
        public static void RunForever(Func<CancellationToken, TimeSpan> task, TimeSpan spanDelay)
        {
            RunForever(token => Task.FromResult(task(token)), spanDelay);
        }

        /// <summary>
        /// Register periodic background task with ASP.Net hosting environment and trace exceptions.
        /// </summary>
        /// <param name="task">Background task to execute.</param>
        /// <param name="spanDelay">The initial delay.</param>
        public static void RunForever(Func<CancellationToken, Task<TimeSpan>> task, TimeSpan spanDelay)
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
