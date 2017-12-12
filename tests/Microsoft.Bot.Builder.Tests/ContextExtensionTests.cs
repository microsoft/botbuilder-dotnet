using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.ContextExtensions;
using Microsoft.Bot.Connector;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("ContextExtensions")]
    public class ContextExtensionTests
    {
        [TestMethod]
        public async Task ContextDelay()
        {
            TestAdapter adapter = new TestAdapter();
            Bot bot = new Bot(adapter)
                .OnReceive(async (context) =>
                {
                    if (context.Request.Text == "wait")
                    {
                        context
                            .Reply("before")
                            .Delay(1000)
                            .Reply("after");
                    }
                    else
                    {
                        context.Reply(context.Request.Text);
                    }
                });

            DateTime start = DateTime.Now;
            await adapter
                .Send("wait")
                    .AssertReply("before")
                    .AssertReply("after")
                .StartTest();

            double duration = (DateTime.Now.Subtract(start)).TotalMilliseconds;

            // The delay should have taken 1000 ms. Assert
            Assert.IsTrue(duration > 500 && duration < 1500, $"Duration not in range: {duration}.");
        }

        [TestMethod]
        public async Task ContextShowTyping()
        {
            TestAdapter adapter = new TestAdapter();
            Bot bot = new Bot(adapter)
                .OnReceive(async (context) =>
                {
                    if (context.Request.Text == "typing")
                    {
                        context.ShowTyping(); 
                        context.Reply("typing done");
                    }
                    else
                    {
                        context.Reply(context.Request.Text);
                    }
                });

            await adapter
                .Send("typing")
                .AssertReply( 
                    (activity)=> { Assert.IsTrue(activity.Type == ActivityTypes.Typing); },
                    "Typing indiciator not set")
                .AssertReply("typing done")
                .StartTest();
        }
    }
}
