// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Connector.Streaming.Tests
{
    internal class SyncPoint
    {
        private readonly TaskCompletionSource<object> _atSyncPoint = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<object> _continueFromSyncPoint = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>
        /// Cretes a sync point and returns the associated handler.
        /// </summary>
        /// <param name="syncPoint">The created <see cref="SyncPoint"/>.</param>
        /// <returns>A <see cref="Func{Task}"/> representing the sync point.</returns>
        public static Func<Task> Create(out SyncPoint syncPoint)
        {
            var handler = Create(1, out var syncPoints);
            syncPoint = syncPoints[0];
            return handler;
        }

        /// <summary>
        /// Creates a re-entrant function that waits for sync points in sequence.
        /// </summary>
        /// <param name="count">The number of sync points to expect.</param>
        /// <param name="syncPoints">The <see cref="SyncPoint"/> objects that can be used to coordinate the sync point.</param>
        /// <returns>A <see cref="Func{Task}"/> representing the sync point next step.</returns>
        public static Func<Task> Create(int count, out SyncPoint[] syncPoints)
        {
            // Need to use a local so the closure can capture it. You can't use out vars in a closure.
            var localSyncPoints = new SyncPoint[count];
            for (var i = 0; i < count; i += 1)
            {
                localSyncPoints[i] = new SyncPoint();
            }

            syncPoints = localSyncPoints;

            var counter = 0;
            return () =>
            {
                if (counter >= localSyncPoints.Length)
                {
                    return Task.CompletedTask;
                }
                else
                {
                    var syncPoint = localSyncPoints[counter];

                    counter += 1;
                    return syncPoint.WaitToContinue();
                }
            };
        }

        /// <summary>
        /// Waits for the code-under-test to reach <see cref="WaitToContinue"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task WaitForSyncPoint() => _atSyncPoint.Task;

        /// <summary>
        /// Releases the code-under-test to continue past where it waited for <see cref="WaitToContinue"/>.
        /// </summary>
        /// <param name="obj">The result of the sync point continuation.</param>
        public void Continue(object obj = null) => _continueFromSyncPoint.TrySetResult(obj);

        /// <summary>
        /// Used by the code-under-test to wait for the test code to sync up.
        /// </summary>
        /// <remarks>
        /// This code will unblock <see cref="WaitForSyncPoint"/> and then block waiting for <see cref="Continue"/> to be called.
        /// </remarks>
        /// <param name="obj">The underlying task result.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task WaitToContinue(object obj = null)
        {
            _atSyncPoint.TrySetResult(obj);
            return _continueFromSyncPoint.Task;
        }
    }
}
