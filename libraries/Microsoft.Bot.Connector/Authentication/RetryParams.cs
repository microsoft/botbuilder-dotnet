// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// Wrapper class that defines a retrying behavior.
    /// </summary>
    public class RetryParams
    {
        private const int MaxRetries = 10;
        private static readonly TimeSpan MaxDelay = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan DefaultBackOffTime = TimeSpan.FromMilliseconds(50);

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryParams"/> class.
        /// </summary>
        public RetryParams()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryParams"/> class.
        /// </summary>
        /// <param name="retryAfter">Timespan to wait between retries.</param>
        /// <param name="shouldRetry">Bool that indicates if a retry is required. The default is true.</param>
        public RetryParams(TimeSpan retryAfter, bool shouldRetry = true)
        {
            ShouldRetry = shouldRetry;
            RetryAfter = retryAfter;

            // We don't allow more than maxDelaySeconds seconds delay.
            if (RetryAfter > MaxDelay)
            {
                // We don't want to throw here though - if the server asks for more delay
                // than we are willing to, just enforce the upper bound for the delay
                RetryAfter = MaxDelay;
            }
        }

        /// <summary>
        /// Gets the property that stops retrying.
        /// </summary>
        /// <value>
        /// The property that stops retrying.
        /// </value>
        public static RetryParams StopRetrying { get; } = new RetryParams() { ShouldRetry = false };

        /// <summary>
        /// Gets or sets a value indicating whether the retry action should be performed.
        /// </summary>
        /// <value>
        /// A value indicating whether the retry action should be performed.
        /// </value>
        public bool ShouldRetry { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the time interval to wait between retry attempts.
        /// </summary>
        /// <value>
        /// A value indicating the time interval to wait between retry attempts.
        /// </value>
        public TimeSpan RetryAfter { get; set; }

        /// <summary>
        /// Evaluates if the current retry count is less than the maximum number of retries allowed, and returns a new
        /// <see cref="RetryParams" /> object if true, or sets the <see cref="RetryParams.StopRetrying"/> property to false if false.
        /// </summary>
        /// <param name="retryCount">The number of times to perform a retry.</param>
        /// <returns>A <see cref="RetryParams"/> object.</returns>
        public static RetryParams DefaultBackOff(int retryCount)
        {
            if (retryCount < MaxRetries)
            {
                return new RetryParams(DefaultBackOffTime);
            }
            else
            {
                return RetryParams.StopRetrying;
            }
        }
    }
}
