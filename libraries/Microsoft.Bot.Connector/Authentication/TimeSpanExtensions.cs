using System;

namespace Microsoft.Bot.Connector.Authentication
{
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
