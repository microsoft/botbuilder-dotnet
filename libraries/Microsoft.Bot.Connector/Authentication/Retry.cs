using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Connector.Authentication
{
    public static class Retry
    {
        public static async Task<TResult> Run<TResult>(Func<Task<TResult>> task, Func<Exception, int, RetryParams> retryExceptionHandler)
        {
            RetryParams retry = RetryParams.StopRetrying;
            List<Exception> exceptions = new List<Exception>();
            int currentRetryCount = 0;

            do
            {
                try
                {
                    return await task().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);

                    retry = retryExceptionHandler(ex, currentRetryCount);
                }

                if (retry.ShouldRetry)
                {
                    currentRetryCount++;
                    await Task.Delay(retry.RetryAfter.WithJitter()).ConfigureAwait(false);
                }

            } while (retry.ShouldRetry);

            throw new AggregateException("Failed to acquire token for client credentials.", exceptions);
        }
    }

    public static class TimeSpanExtensions
    {
        private static Random random = new Random();

        public static TimeSpan WithJitter(this TimeSpan delay, double multiplier = 0.1)
        {
            // Generate an uniform distribution between zero and 10% of the proposed delay and add it as 
            // random noise. The reason for this is that if there are multiple threads about to retry
            // at the same time, it can overload the server again and trigger throttling again.
            // By adding a bit of random noise, we distribute requests a across time.
            return delay + TimeSpan.FromMilliseconds(random.NextDouble() * delay.TotalMilliseconds * 0.1);
        }
    }
}
