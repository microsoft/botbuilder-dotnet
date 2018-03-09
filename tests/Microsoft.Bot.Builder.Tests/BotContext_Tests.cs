// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests
{


    public class AnnotateMiddleware : IContextCreated, IReceiveActivity, ISendActivity
    {
        public async Task SendActivity(BotContext context, IList<Activity> activities) {; }
        public async Task ContextDone(BotContext context) { /*context.State["ContextDone"] = true;*/ }

        public async Task ContextCreated(IBotContext context, MiddlewareSet.NextDelegate next)
        {
            //context.State["ContextCreated"] = true;
            await next();
        }

        public async Task ReceiveActivity(IBotContext context, MiddlewareSet.NextDelegate next)
        {
            context.Request.AsMessageActivity().Text += "ReceiveActivity";
            await next();
        }
        public async Task SendActivity(IBotContext context, IList<Activity> activities, MiddlewareSet.NextDelegate next)
        {
            if (context.Responses.Count > 0)
            {
                context.Responses[0].AsMessageActivity().Text += "SendActivity";
            }
            await next();
        }
    }

    [TestClass]
    [TestCategory("Middleware")]
    public class BotContext_Tests
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
        public async Task TestProactivePipeline()
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
        public async Task Context_ReplyTextOnly()
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

        

       
    }
}