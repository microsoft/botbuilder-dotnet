// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Connector.Authentication
{
    public static class Retry
    {
        public static async Task<TResult> Run<TResult>(Func<Task<TResult>> task, Func<Exception, int, RetryParams> retryExceptionHandler)
        {
            RetryParams retry;
            var exceptions = new List<Exception>();
            var currentRetryCount = 0;

            do
            {
                try
                {
                    return await task().ConfigureAwait(false);
                }
#pragma warning disable CA1031 // Do not catch general exception types (this is a generic catch all to handle retries)
                catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    exceptions.Add(ex);
                    retry = retryExceptionHandler(ex, currentRetryCount);
                }

                if (retry.ShouldRetry)
                {
                    currentRetryCount++;
                    await Task.Delay(retry.RetryAfter.WithJitter()).ConfigureAwait(false);
                }
            }
            while (retry.ShouldRetry);

            throw new AggregateException("Failed to acquire token for client credentials.", exceptions);
        }
    }
}
