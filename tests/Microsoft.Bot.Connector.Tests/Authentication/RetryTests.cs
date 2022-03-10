// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Xunit;

namespace Microsoft.Bot.Connector.Tests.Authentication
{
    public class RetryTests
    {
        [Fact]
        public async Task Retry_NoRetryWhenTaskSucceeds()
        {
            FaultyClass faultyClass = new FaultyClass()
            {
                ExceptionToThrow = null,
            };

            var result = await Retry.RunAsync(
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
                TriesUntilSuccess = 3,
            };

            var result = await Retry.RunAsync(
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
                TriesUntilSuccess = 12,
            };

            await Assert.ThrowsAsync<AggregateException>(async () =>
                await Retry.RunAsync(
                    task: () => faultyClass.FaultyTask(),
                    retryExceptionHandler: (ex, ct) => faultyClass.ExceptionHandler(ex, ct)));
        }
    }
}
