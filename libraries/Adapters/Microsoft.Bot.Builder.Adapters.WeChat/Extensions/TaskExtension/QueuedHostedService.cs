using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters.WeChat;

namespace Microsoft.Microsoft.Bot.Builder.Adapters.WeChat.TaskExtensions
{
    public class QueuedHostedService : BackgroundService
    {
        private readonly WeChatLogger logger;

        public QueuedHostedService(IBackgroundTaskQueue queue, WeChatLogger logger = null)
        {
            this.TaskQueue = queue;
            this.logger = logger ?? WeChatLogger.Instance;
        }

        public IBackgroundTaskQueue TaskQueue { get; set; }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var workItem = await this.TaskQueue.DequeueAsync(stoppingToken).ConfigureAwait(false);

                try
                {
                    await workItem(stoppingToken).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    // execution failed. added exception handling later
                    // only log the exception for now
                    this.logger.TrackException("Execute Background Queued Task Failed", e);
                }
            }
        }
    }
}