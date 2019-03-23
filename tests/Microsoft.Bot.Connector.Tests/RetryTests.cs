using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Xunit;

namespace Microsoft.Bot.Connector.Tests
{
    public class RetryTests
    {
        [Fact]
        public async Task Retry_NoRetryWhenTaskSucceeds()
        {
            FaultyClass faultyClass = new FaultyClass()
            {
                ExceptionToThrow = null
            };

            var result = await Retry.Run(
                task: () => faultyClass.FaultyTask(),
                retryExceptionHandler: (ex, ct) => faultyClass.ExceptionHandler(ex, ct));

            Assert.Null(faultyClass.ExceptionReceived);
            Assert.Equal(1, faultyClass.CallCount);
        }

        [Fact]
        public async Task Retry_RetryThenSucceed()
        {
            FaultyClass faultyClass = new FaultyClass()
            {
                ExceptionToThrow = new ArgumentNullException(),
                TriesUntilSuccess = 3
            };

            var result = await Retry.Run(
                task: () => faultyClass.FaultyTask(),
                retryExceptionHandler: (ex, ct) => faultyClass.ExceptionHandler(ex, ct));

            Assert.NotNull(faultyClass.ExceptionReceived);
            Assert.Equal(3, faultyClass.CallCount);
        }

        [Fact]
        public async Task Retry_RetryUntilFailure()
        {
            FaultyClass faultyClass = new FaultyClass()
            {
                ExceptionToThrow = new ArgumentNullException(),
                TriesUntilSuccess = 12
            };

            await Assert.ThrowsAsync<AggregateException>(async () => 
                await Retry.Run(
                    task: () => faultyClass.FaultyTask(),
                    retryExceptionHandler: (ex, ct) => faultyClass.ExceptionHandler(ex, ct)));
        }
    }

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

            return string.Empty;
        }

        public RetryParams ExceptionHandler(Exception ex, int currentRetryCount)
        {
            ExceptionReceived = ex;
            LatestRetryCount = currentRetryCount;

            return RetryParams.DefaultBackOff(currentRetryCount);
        }
    }
}
