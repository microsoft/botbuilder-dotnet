// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Servers;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests
{

    public class AnnotateMiddleware : IContextCreated, IReceiveActivity, ISendActivity
    {
        public async Task SendActivity(BotContext context, IList<Activity> activities) {; }
        public async Task ContextDone(BotContext context) { context.State["ContextDone"] = true; }

        public async Task ContextCreated(IBotContext context, MiddlewareSet.NextDelegate next)
        {
            context.State["ContextCreated"] = true;
            await next();
        }

        public async Task ReceiveActivity(IBotContext context, MiddlewareSet.NextDelegate next)
        {
            context.Request.AsMessageActivity().Text += "ReceiveActivity";
            await next();
        }
        public async Task SendActivity(IBotContext context, IList<IActivity> activities, MiddlewareSet.NextDelegate next)
        {
            context.Responses[0].AsMessageActivity().Text += "SendActivity";
            await next();
        }
    }

    [TestClass]
    [TestCategory("Middleware")]
    public class BotContext_Tests
    {
        private TestBotServer CreateBotServer()
        {

            TestBotServer botServer = new TestBotServer();
            botServer = botServer
                .Use(new AnnotateMiddleware());
            return botServer;
        }

        public async Task MyCodeHandler(IBotContext context)
        {
            Assert.AreEqual(true, context.State["ContextCreated"]);
            Assert.IsTrue(context.Request.AsMessageActivity().Text.Contains("ReceiveActivity"));
            Assert.IsFalse(context.Request.AsMessageActivity().Text.Contains("SendActivity"));
            if (context.Request.AsMessageActivity().Text.StartsWith("proactive"))
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                var reference = context.ConversationReference;
                Task.Run(async () =>
                {
                    await Task.Delay(1000).ConfigureAwait(false);
                    await context.Bot.ContinueConversation(reference, async (context2) =>
                    {
                        Assert.AreEqual(true, context2.State["ContextCreated"]);
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
        public async Task TestReceivePipeline()
        {
            var botServer = CreateBotServer();
            await new TestFlow(botServer, MyCodeHandler)
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
        public async Task TestProactivePipeline()
        {
            var botServer = CreateBotServer();
            await new TestFlow(botServer, MyCodeHandler)
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
        public async Task Context_ReplyTextOnly()
        {

            TestBotServer botServer = new TestBotServer();
            await new TestFlow(botServer, async (context) =>
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
        public async Task Context_ReplyTextAndSSML()
        {

            string ssml = @"<speak><p>hello</p></speak>";

            TestBotServer botServer = new TestBotServer();

            await new TestFlow(botServer, async (context) =>
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
        public async Task Context_ReplyActivity()
        {

            TestBotServer botServer = new TestBotServer();
            await new TestFlow(botServer, async (context) =>
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