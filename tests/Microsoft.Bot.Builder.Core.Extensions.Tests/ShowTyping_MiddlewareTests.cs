using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Core.Extensions.Tests
{
    [TestClass]
    public class ShowTyping_MiddlewareTests
    {
        [TestMethod]
        [TestCategory("Middleware")]
        public async Task ShowTyping_TestMiddleware_10SecondResponse_2SecondInterval()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ShowTypingMiddleware(500, 2000));
            
            await new TestFlow(adapter, async (context) =>
                {
                    Thread.Sleep(10000);
                    await context.SendActivity(context.Request.CreateReply("Message sent after delay"));
                    await Task.CompletedTask;
                })
                .Send("foo")
                .AssertReply(ValidateTypingActivity, "check typing activity")
                .AssertReply(ValidateTypingActivity, "check typing activity")
                .AssertReply(ValidateTypingActivity, "check typing activity")
                .AssertReply(ValidateTypingActivity, "check typing activity")
                .AssertReply(ValidateTypingActivity, "check typing activity")
                .AssertReply("Message sent after delay")
                .StartTest();
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task ShowTyping_TestMiddleware_10SecondResponse_5SecondInterval()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ShowTypingMiddleware(500, 5000));

            await new TestFlow(adapter, async (context) =>
                {
                    Thread.Sleep(10000);
                    await context.SendActivity(context.Request.CreateReply("Message sent after delay"));
                    await Task.CompletedTask;
                })
                .Send("foo")
                .AssertReply(ValidateTypingActivity, "check typing activity")
                .AssertReply(ValidateTypingActivity, "check typing activity")
                .AssertReply("Message sent after delay")
                .StartTest();
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task ShowTyping_TestMiddleware_1SecondResponse_5SecondInterval()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ShowTypingMiddleware(500, 5000));

            await new TestFlow(adapter, async (context) =>
                {
                    Thread.Sleep(1000);
                    await context.SendActivity(context.Request.CreateReply("Message sent after delay"));
                    await Task.CompletedTask;
                })
                .Send("foo")
                .AssertReply(ValidateTypingActivity, "check typing activity")
                .AssertReply("Message sent after delay")
                .StartTest();
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task ShowTyping_TestMiddleware_ImmediateResponse_5SecondInterval()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ShowTypingMiddleware(500, 5000));

            await new TestFlow(adapter, async (context) =>
                {
                    await context.SendActivity(context.Request.CreateReply("Message sent after delay"));
                    await Task.CompletedTask;
                })
                .Send("foo")
                .AssertReply("Message sent after delay")
                .StartTest();
        }

        private void ValidateTypingActivity(IActivity obj)
        {
            var activity = obj.AsTypingActivity();
            if (activity != null)
            {
                return;
            }
            else
            {
                throw new Exception("Activity was not of type TypingActivity");
            }
        }
    }
}
