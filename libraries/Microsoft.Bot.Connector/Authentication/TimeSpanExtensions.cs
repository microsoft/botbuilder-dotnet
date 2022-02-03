// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// Extention methods for the <see cref="TimeSpan"/> class.
    /// </summary>
    public static class TimeSpanExtensions
    {
        private static readonly Random _random = new Random();

        /// <summary>
        /// Generates a uniform distribution between zero and ten percent of the proposed delay and adds it
        /// to the delay as random noise to distribute requests across time.
        /// </summary>
        /// <param name="delay">The requested delay.</param>
        /// <returns>A uniformly distributed time span.</returns>
        public static TimeSpan WithJitter(this TimeSpan delay)
        {
            // Generate an uniform distribution between zero and 10% of the proposed delay and add it as
            // random noise. The reason for this is that if there are multiple threads about to retry
            // at the same time, it can overload the server again and trigger throttling again.
            // By adding a bit of random noise, we distribute requests a across time.
            return delay + TimeSpan.FromMilliseconds(_random.NextDouble() * delay.TotalMilliseconds * 0.1);
        }
    }
}
