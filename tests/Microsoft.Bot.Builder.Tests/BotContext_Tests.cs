using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Connector;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Tests
{

    public class AnnotateMiddleware : IContextCreated, IReceiveActivity, IPostActivity, IContextDone
    {
        public async Task ContextCreated(BotContext context) { context.State["ContextCreated"] = true; }
        public async Task<ReceiveResponse> ReceiveActivity(BotContext context) { context.Request.Text += "ReceiveActivity"; return new ReceiveResponse(false); }
        public async Task PostActivity(BotContext context, IList<Activity> activities) { context.Responses[0].Text += "PostActivity"; }
        public async Task ContextDone(BotContext context) { context.State["ContextDone"] = true; }
    }

    [TestClass]
    [TestCategory("Middleware")]
    public class BotContext_Tests
    {
        private TestAdapter CreateAdapter()
        {
            TestAdapter adapter = new TestAdapter();
            Bot bot = new Bot(adapter);
            bot = bot
                .Use(new AnnotateMiddleware())
                .OnReceive(
                    async (context) =>
                    {
                        Assert.AreEqual(true, context.State["ContextCreated"]);
                        Assert.IsTrue(context.Request.Text.Contains("ReceiveActivity"));
                        Assert.IsFalse(context.Request.Text.Contains("PostActivity"));
                        if (context.Request.Text.StartsWith("proactive"))
                        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                            var reference = context.ConversationReference;
                            Task.Run(async () =>
                            {
                                await Task.Delay(1000).ConfigureAwait(false);
                                await bot.CreateContext(reference, async (context2) =>
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
                            context.Reply(context.Request.Text);
                        }
                    }
                );
            return adapter;
        }


        [TestMethod]
        public async Task TestReceivePipeline()
        {
            await CreateAdapter()
                .Send("receive")
                .AssertReply((activity) =>
                {
                    Assert.IsTrue(activity.Text.Contains("receive"));
                    Assert.IsTrue(activity.Text.Contains("ReceiveActivity"));
                    Assert.IsTrue(activity.Text.Contains("PostActivity"));
                }, "Assert response came through")
                .StartTest();
        }

        [TestMethod]
        public async Task TestProactivePipeline()
        {
            await CreateAdapter()
                .Send("proactive")
                .AssertReply((activity) =>
                {
                    Assert.IsTrue(activity.Text.Contains("proactive"));
                    Assert.IsFalse(activity.Text.Contains("ReceiveActivity"));
                    Assert.IsTrue(activity.Text.Contains("PostActivity"));
                }, "Assert response came through")
               .StartTest();
        }
    }

}
