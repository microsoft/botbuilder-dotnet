using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests.Adapters
{
    [TestClass]
    public class TestFlowTests
    {
        [TestMethod]
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

        [TestMethod]
        public async Task ValidateReplyContains_ExceptionWithDescription()
        {
            const string exceptionDescription = "Description message";
            const string stringThatNotSubstring = "some string";
            var message = "Just a sample string".Replace(stringThatNotSubstring, string.Empty);
            try
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
            }
            catch (Exception e)
            {
                if (!e.Message.Contains(exceptionDescription))
                {
                    throw;
                }
            }
        }

        [TestMethod]
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

            Assert.IsTrue(sw.Elapsed.TotalSeconds > 1, $"Delay broken, elapsed time {sw.Elapsed}?");
        }
    }
}
