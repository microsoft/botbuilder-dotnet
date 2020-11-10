using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Xunit;

namespace Microsoft.Bot.Builder.Tests.Adapters
{
    public class TestFlowTests
    {
        [Fact]
        public async Task ValidateReplyContains()
        {
            var expectedSubstring = "expected substring";
            await new TestFlow(new TestAdapter(), async (turnContext, cancellationToken) =>
                {
                    await turnContext.SendActivityAsync(
                        $"String with {expectedSubstring} in it",
                        cancellationToken: cancellationToken);
                })
                .Send("hello")
                .AssertReplyContains(expectedSubstring)
                .StartTestAsync();
        }

        [Fact]
        public async Task ValidateReplyContains_ExceptionWithDescription()
        {
            const string exceptionDescription = "Description message";
            const string stringThatNotSubstring = "some string";
            var message = "Just a sample string".Replace(stringThatNotSubstring, string.Empty);
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await new TestFlow(new TestAdapter(), async (turnContext, cancellationToken) =>
                    {
                        await turnContext.SendActivityAsync(
                            message,
                            cancellationToken: cancellationToken);
                    })
                    .Send("hello")
                    .AssertReplyContains(stringThatNotSubstring, exceptionDescription)
                    .StartTestAsync();
            });
        }

        [Fact]
        public async Task ValidateDelay()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            await new TestFlow(new TestAdapter())
            .Send("hello")
            .Delay(TimeSpan.FromSeconds(1.1))
            .Send("some text")
            .StartTestAsync();
            sw.Stop();

            Assert.True(sw.Elapsed.TotalSeconds > 1, $"Delay broken, elapsed time {sw.Elapsed}?");
        }

        [Fact]
        public async Task ValidateNoReply()
        {
            const string message = "Just a sample string";
            await new TestFlow(new TestAdapter(), async (turnContext, cancellationToken) =>
                {
                    await turnContext.SendActivityAsync(
                        message,
                        cancellationToken: cancellationToken);
                })
                .Send("hello")
                .AssertReply(message)
                .AssertNoReply()
                .StartTestAsync();
        }

        [Fact]
        public async Task ValidateNoReply_ExceptionWithDescription()
        {
            const string exceptionDescription = "Description message";
            const string message = "Just a sample string";
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await new TestFlow(new TestAdapter(), async (turnContext, cancellationToken) =>
                    {
                        await turnContext.SendActivityAsync(
                            message,
                            cancellationToken: cancellationToken);
                        await turnContext.SendActivityAsync(
                            message,
                            cancellationToken: cancellationToken);
                    })
                    .Send("hello")
                    .AssertReply(message)
                    .AssertNoReply(exceptionDescription)
                    .StartTestAsync();
            });
        }
    }
}
