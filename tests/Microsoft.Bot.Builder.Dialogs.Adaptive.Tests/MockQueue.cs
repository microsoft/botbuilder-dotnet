using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    public class MockQueue : QueueStorage
    {
        private int receipt = 1;

        private Queue<Activity> queue = new Queue<Activity>();

        public override Task<string> QueueActivityAsync(Activity activity, TimeSpan? visibilityTimeout = null, TimeSpan? timeToLive = null, CancellationToken cancellationToken = default)
        {
            if (visibilityTimeout != null)
            {
                Task.Delay(visibilityTimeout.Value).ContinueWith(t =>
                {
                    lock (queue)
                    {
                        queue.Enqueue(activity);
                    }
                });
            }
            else
            {
                lock (this.queue)
                {
                    queue.Enqueue(activity);
                }
            }

            return Task.FromResult($"{receipt++}");
        }

        public async Task<Activity> ReceiveActivity()
        {
            while (true)
            {
                lock (queue)
                {
                    if (queue.Any())
                    {
                        return queue.Dequeue();
                    }
                }

                await Task.Delay(1);
            }
        }
    }
}
