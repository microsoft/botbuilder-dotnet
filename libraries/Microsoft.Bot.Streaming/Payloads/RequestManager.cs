// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Streaming.Payloads
{
    /// <summary>
    ///  Manages the tasks involved in processing and responding to incoming <see cref="StreamingRequest"/>s.
    /// </summary>
    public class RequestManager : IRequestManager
    {
        private readonly ConcurrentDictionary<Guid, TaskCompletionSource<ReceiveResponse>> _responseTasks;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestManager"/> class.
        /// </summary>
        public RequestManager()
            : this(new ConcurrentDictionary<Guid, TaskCompletionSource<ReceiveResponse>>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestManager"/> class.
        /// </summary>
        /// <param name="responseTasks">A set of tasks to manage.</param>
        public RequestManager(ConcurrentDictionary<Guid, TaskCompletionSource<ReceiveResponse>> responseTasks)
        {
            _responseTasks = responseTasks;
        }

        /// <inheritdoc/>
        public Task<bool> SignalResponseAsync(Guid requestId, ReceiveResponse response)
        {
            if (_responseTasks.TryGetValue(requestId, out TaskCompletionSource<ReceiveResponse> signal))
            {
#pragma warning disable VSTHRD110 // Observe result of async calls
                Task.Run(() => { signal.TrySetResult(response); });
#pragma warning restore VSTHRD110 // Observe result of async calls
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public async Task<ReceiveResponse> GetResponseAsync(Guid requestId, CancellationToken cancellationToken)
        {
            TaskCompletionSource<ReceiveResponse> responseTask = new TaskCompletionSource<ReceiveResponse>();

            if (!_responseTasks.TryAdd(requestId, responseTask))
            {
                return null;
            }

            if (cancellationToken == null)
            {
                cancellationToken = CancellationToken.None;
            }

            try
            {
                using (cancellationToken.Register(() =>
                {
                    responseTask.TrySetCanceled();
                }))
                {
                    var response = await responseTask.Task.ConfigureAwait(false);
                    return response;
                }
            }
            finally
            {
                _responseTasks.TryRemove(requestId, out responseTask);
            }
        }
    }
}
