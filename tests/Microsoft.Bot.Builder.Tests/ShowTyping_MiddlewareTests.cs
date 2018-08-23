// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    public class ShowTyping_MiddlewareTests
    {
        [TestMethod]
        [TestCategory("Middleware")]
        public async Task ShowTyping_TestMiddleware_1_Second_Interval()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ShowTypingMiddleware(100, 1000));
            
            await new TestFlow(adapter, async (context, cancellationToken) =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(2500));
                    await context.SendActivityAsync("Message sent after delay");
                    await Task.CompletedTask;
                })
                .Send("foo")
                .AssertReply(ValidateTypingActivity, "check typing activity")
                .AssertReply(ValidateTypingActivity, "check typing activity")
                .AssertReply(ValidateTypingActivity, "check typing activity")
                .AssertReply("Message sent after delay")
                .StartTestAsync();
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task ShowTyping_TestMiddleware_Context_Completes_Before_Typing_Interval()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ShowTypingMiddleware(100, 5000));

            await new TestFlow(adapter, async (context, cancellationToken) =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(2000));
                    await context.SendActivityAsync("Message sent after delay");
                    await Task.CompletedTask;
                })
                .Send("foo")
                .AssertReply(ValidateTypingActivity, "check typing activity")
                .AssertReply("Message sent after delay")
                .StartTestAsync();
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task ShowTyping_TestMiddleware_ImmediateResponse_5SecondInterval()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ShowTypingMiddleware(2000, 5000));

            await new TestFlow(adapter, async (context, cancellationToken) =>
                {
                    await context.SendActivityAsync("Message sent after delay");
                    await Task.CompletedTask;
                })
                .Send("foo")
                .AssertReply("Message sent after delay")
                .StartTestAsync();
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public void ShowTyping_TestMiddleware_NegativeDelay()
        {
            try
            {
                TestAdapter adapter = new TestAdapter()
                    .Use(new ShowTypingMiddleware(-100, 1000));
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ArgumentOutOfRangeException));
            }
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public void ShowTyping_TestMiddleware_ZeroFrequency()
        {
            try
            {
                TestAdapter adapter = new TestAdapter()
                    .Use(new ShowTypingMiddleware(-100, 0));
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ArgumentOutOfRangeException));
            }
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
