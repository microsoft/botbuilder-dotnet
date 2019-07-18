using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Adapters.WeChat.TaskExtensions
{
    public class QueuedHostedService : AdapterBackgroundService
    {
        private readonly WeChatLogger logger;

        public QueuedHostedService(IBackgroundTaskQueue queue = null, WeChatLogger logger = null)
        {
            this.TaskQueue = queue ?? BackgroundTaskQueue.Instance;
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