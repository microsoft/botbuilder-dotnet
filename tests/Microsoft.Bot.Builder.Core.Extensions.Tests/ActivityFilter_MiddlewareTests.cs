using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Core.Extensions.Tests
{
    [TestClass]
    public class ActivityFilter_MiddlewareTests
    {
        [TestMethod]
        [TestCategory("Middleware")]
        public async Task ActivityFilter_TestMiddleware_HandleActivityAndContinue()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ActivityFilterMiddleware(ActivityTypes.Message, async (context, next) =>
                {
                    await context.SendActivity(context.Request.CreateReply("Handling a message activity"));
                    await next();
                }));


            await new TestFlow(adapter, async (context) =>
                {
                    await context.SendActivity(context.Request.CreateReply("Follow up message from bot"));
                    await Task.CompletedTask;
                })
                .Send("foo")
                .AssertReply("Handling a message activity")
                .AssertReply("Follow up message from bot")
                .StartTest();
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task ActivityFilter_TestMiddleware_HandleMessageActivityOnly()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ActivityFilterMiddleware(ActivityTypes.ConversationUpdate, async (context, next) =>
                {
                    await context.SendActivity(context.Request.CreateReply("Handling conversation update"));
                }))
                .Use(new ActivityFilterMiddleware(ActivityTypes.Message, async (context, next) =>
                {
                    await context.SendActivity(context.Request.CreateReply("Handling a message activity"));
                    await next();
                })); 


            await new TestFlow(adapter, async (context) =>
            {
                await context.SendActivity(context.Request.CreateReply("Follow up message from bot"));
                await Task.CompletedTask;
            })
                .Send("foo")
                .AssertReply("Handling a message activity")
                .StartTest();
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task ActivityFilter_TestMiddleware_MultipleHandlers()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ActivityFilterMiddleware(ActivityTypes.Message, async (context, next) =>
                {
                    await context.SendActivity(context.Request.CreateReply("Handling a message activity"));
                    await next();
                }))
                .Use(new ActivityFilterMiddleware(ActivityTypes.Message, async (context, next) =>
                {
                    await context.SendActivity(context.Request.CreateReply("Handling a message activity"));
                    await next();
                }));


            await new TestFlow(adapter, async (context) =>
                {
                    await context.SendActivity(context.Request.CreateReply("Follow up message from bot"));
                    await Task.CompletedTask;
                })
                .Send("foo")
                .AssertReply("Handling a message activity")
                .AssertReply("Handling a message activity")
                .AssertReply("Follow up message from bot")
                .StartTest();
        }

    }
}
