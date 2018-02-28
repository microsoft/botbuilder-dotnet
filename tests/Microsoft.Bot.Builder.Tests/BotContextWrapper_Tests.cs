// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Microsoft.Bot.Builder.Tests
{

    public class TestBotContext : BotContextWrapper
    {
        public TestBotContext(IBotContext context) : base(context) { }

        public void ReplyTwice(string text)
        {
            this.Reply(text);
            this.Reply(text);
        }
    }


    [TestClass]
    [TestCategory("Middleware")]
    public class BotContextWrapper_Tests
    {
        private TestAdapter CreateBotAdapter()
        {

            TestAdapter adapter = new TestAdapter();
            adapter = adapter
                .Use(new AnnotateMiddleware());
            return adapter;
        }

        public async Task MyCodeHandler(IBotContext context)
        {
            context = new TestBotContext(context);

            Assert.IsTrue(context.Request.AsMessageActivity().Text.Contains("ReceiveActivity"));
            Assert.IsFalse(context.Request.AsMessageActivity().Text.Contains("SendActivity"));
            if (context.Request.AsMessageActivity().Text.StartsWith("proactive"))
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                var reference = context.ConversationReference;
                Task.Run(async () =>
                {
                    await Task.Delay(1000).ConfigureAwait(false);
                    await context.Adapter.ContinueConversation(reference, async (context2) =>
                    {
                        context2 = new TestBotContext(context2);
                        Assert.IsNull(context2.Request);
                        context2.Reply("proactive");
                    }).ConfigureAwait(false);
                });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                return;
            }
            else
            {
                context.Reply(context.Request.AsMessageActivity().Text);
            }
        }

        [TestMethod]
        public async Task BotContextWrapper_TestReceivePipeline()
        {
            var adapter = CreateBotAdapter();
            await new TestFlow(adapter, MyCodeHandler)
                .Send("receive")
                .AssertReply((activity) =>
                {
                    Assert.IsTrue(activity.AsMessageActivity().Text.Contains("receive"));
                    Assert.IsTrue(activity.AsMessageActivity().Text.Contains("ReceiveActivity"));
                    Assert.IsTrue(activity.AsMessageActivity().Text.Contains("SendActivity"));
                }, "Assert response came through")
                .StartTest();
        }

        [TestMethod]
        public async Task BotContextWrapper_TestProactivePipeline()
        {
            var adapter = CreateBotAdapter();
            await new TestFlow(adapter, MyCodeHandler)
                .Send("proactive")
                .AssertReply((activity) =>
                {
                    Assert.IsTrue(activity.AsMessageActivity().Text.Contains("proactive"));
                    Assert.IsFalse(activity.AsMessageActivity().Text.Contains("ReceiveActivity"));
                    Assert.IsTrue(activity.AsMessageActivity().Text.Contains("SendActivity"));
                }, "Assert response came through")
               .StartTest();
        }

        [TestMethod]
        [TestCategory("Functional Spec")]
        public async Task BotContextWrapper_ReplyTextOnly()
        {

            TestAdapter adapter = new TestAdapter();
            await new TestFlow(adapter, async (context) =>
            {
                if (context.Request.AsMessageActivity().Text == "hello")
                {
                    context.Reply("world");
                }
            })
                .Send("hello")
                    .AssertReply("world")
                .StartTest();
        }

        [TestMethod]
        [TestCategory("Functional Spec")]
        public async Task BotContextWrapper_ReplyTextAndSSML()
        {

            string ssml = @"<speak><p>hello</p></speak>";

            TestAdapter adapter = new TestAdapter();

            await new TestFlow(adapter, async (context) =>
            {
                if (context.Request.AsMessageActivity().Text == "hello")
                {
                    context.Reply("use ssml", ssml);
                }
            })
            .Send("hello").AssertReply(
                    (activity) =>
                    {
                        Assert.AreEqual("use ssml", activity.AsMessageActivity().Text);
                        Assert.AreEqual(ssml, activity.AsMessageActivity().Speak);
                    }
                    , "send/reply with speak text works")
            .StartTest();
        }

        [TestMethod]
        [TestCategory("Functional Spec")]
        public async Task BotContextWrapper_ReplyActivity()
        {

            TestAdapter adapter = new TestAdapter();
            await new TestFlow(adapter, async (context) =>
            {
                if (context.Request.AsMessageActivity().Text == "hello")
                {
                    IActivity reply = context.ConversationReference.GetPostToUserMessage();
                    reply.AsMessageActivity().Text = "world";
                    context.Reply(reply);
                }
            })
                .Send("hello")
                    .AssertReply("world", "send/reply with Activity works")
                .StartTest();
        }
    }
}

