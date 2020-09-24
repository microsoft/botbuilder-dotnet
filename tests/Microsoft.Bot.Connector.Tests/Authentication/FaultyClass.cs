// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;

namespace Microsoft.Bot.Connector.Tests.Authentication
{
    public class FaultyClass
    {
        public Exception ExceptionToThrow { get; set; }

        public Exception ExceptionReceived { get; set; } = null;

        public int LatestRetryCount { get; set; }

        public int CallCount { get; set; } = 0;

        public int TriesUntilSuccess { get; set; } = 0;

        public async Task<string> FaultyTask()
        {
            CallCount++;

            if (CallCount < TriesUntilSuccess && ExceptionToThrow != null)
            {
                throw ExceptionToThrow;
            }

            return await Task.FromResult(string.Empty);
        }

        public RetryParams ExceptionHandler(Exception ex, int currentRetryCount)
        {
            ExceptionReceived = ex;
            LatestRetryCount = currentRetryCount;

            return RetryParams.DefaultBackOff(currentRetryCount);
        }
    }
}
