// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Servers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("ContextExtensions")]
    public class ContextExtensionTests
    {
        [TestMethod]
        public async Task ContextDelay()
        {

            TestBotServer botServer = new TestBotServer();

            DateTime start = DateTime.Now;
            await new TestFlow(botServer, async (context) =>
            {
                if (context.Request.AsMessageActivity().Text == "wait")
                {
                    context
                        .Reply("before")
                        .Delay(1000)
                        .Reply("after");
                }
                else
                {
                    context.Reply(context.Request.AsMessageActivity().Text);
                }
            })
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

            TestBotServer botServer = new TestBotServer();

            await new TestFlow(botServer, async (context) =>
                {
                    if (context.Request.AsMessageActivity().Text == "typing")
                    {
                        context.ShowTyping();
                        context.Reply("typing done");
                    }
                    else
                    {
                        context.Reply(context.Request.AsMessageActivity().Text);
                    }
                })
                .Send("typing")
                .AssertReply(
                    (activity) => { Assert.IsTrue(activity.Type == ActivityTypes.Typing); },
                    "Typing indiciator not set")
                .AssertReply("typing done")
                .StartTest();
        }
    }
}
