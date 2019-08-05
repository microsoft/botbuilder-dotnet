// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.Bot.Builder.Adapters.WeChat.TaskExtensions
{
    public class QueuedHostedService : AdapterBackgroundService
    {
        private readonly ILogger _logger;

        public QueuedHostedService(IBackgroundTaskQueue queue = null, ILogger logger = null)
        {
            TaskQueue = queue ?? BackgroundTaskQueue.Instance;
            _logger = logger ?? NullLogger.Instance;
        }

        public IBackgroundTaskQueue TaskQueue { get; set; }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var workItem = await TaskQueue.DequeueAsync(stoppingToken).ConfigureAwait(false);

                try
                {
                    await workItem(stoppingToken).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    // execution failed. added exception handling later
                    // only log the exception for now
                    _logger.LogError(e, "Execute Background Queued Task Failed");
                }
            }
        }
    }
}
