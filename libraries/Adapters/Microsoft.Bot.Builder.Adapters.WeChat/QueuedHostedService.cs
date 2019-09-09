// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.Bot.Builder.Adapters.WeChat
{
    public class QueuedHostedService : BackgroundService
    {
        private readonly ILogger _logger;

        public QueuedHostedService(IBackgroundTaskQueue backgroundTaskQueue, ILogger logger = null)
        {
            TaskQueue = backgroundTaskQueue;
            _logger = logger ?? NullLogger.Instance;
        }

        public IBackgroundTaskQueue TaskQueue { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var workItem = await TaskQueue.DequeueAsync(stoppingToken).ConfigureAwait(false);

                try
                {
                    await workItem(stoppingToken).ConfigureAwait(false);
                }
#pragma warning disable CA1031 // Do not throw exception, you will always need the hosted service.
                catch (Exception e)
#pragma warning restore CA1031
                {
                    // Execute work item in adapter background service.
                    // Typically work item is about convert activity to WeChat message and send it.
                    // Log and rethrow the exception to developer.
                    _logger.LogError(e, "Execute Background Queued Task Failed.");
                }
            }
        }
    }
}
