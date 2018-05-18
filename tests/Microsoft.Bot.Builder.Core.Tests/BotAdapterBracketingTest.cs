using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Core.Extensions.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Core.Tests
{
    [TestClass]
    [TestCategory("Middleware")]
    public class BotAdapterBracketingTest
    {
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Developer authored Middleware that looks like this:
        /// public async Task ReceiveActivity(ITurnContext context, 
        ///    MiddlewareSet.NextDelegate next)
        /// {
        ///    context.Reply("BEFORE");
        ///    await next();   // User Says Hello
        ///    context.Reply("AFTER");
        ///  }
        ///  Should result in an output that looks like:
        ///    BEFORE
        ///    ECHO:Hello
        ///    AFTER        
        /// </summary>       
        [TestMethod]
        public async Task Middlware_BracketingValidation()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            TestAdapter adapter = new TestAdapter()
                .Use(new BeforeAfterMiddleware());

            var flow = new TestFlow(adapter, async (context) => {
                var toEcho = $"ECHO:{context.Activity.AsMessageActivity().Text}";
                await context.SendActivity(toEcho);
            });

            await flow.Test(activities).StartTest();
        }

        /// <summary>
        /// Exceptions thrown during the processing of an Activity should
        /// be catchable by Middleware that has wrapped the next() method. 
        /// This tests verifies that, and makes sure the order of messages
        /// coming back is correct. 
        /// </summary>       
        [TestMethod]
        public async Task Middlware_ThrowException()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            TestAdapter adapter = new TestAdapter()
                .Use(new CatchExceptionMiddleware());

            var flow = new TestFlow(adapter, async (context) => {
                var toEcho = $"ECHO:{context.Activity.AsMessageActivity().Text}";
                await context.SendActivity(toEcho);
                throw new Exception("Error");
            });

            await flow.Test(activities).StartTest();
        }

        public class CatchExceptionMiddleware : IMiddleware
        {
            public async Task OnTurn(ITurnContext context, MiddlewareSet.NextDelegate next)
            {
                await context.SendActivity(context.Activity.CreateReply("BEFORE"));
                try
                {
                    await next();
                }
                catch (Exception ex)
                {
                    await context.SendActivity(context.Activity.CreateReply("CAUGHT:" + ex.Message));                    
                }

                await context.SendActivity(context.Activity.CreateReply("AFTER"));
            }
        }

        public class BeforeAfterMiddleware : IMiddleware
        {
            public async Task OnTurn(ITurnContext context, MiddlewareSet.NextDelegate next)
            {
                await context.SendActivity(context.Activity.CreateReply("BEFORE"));
                await next();
                await context.SendActivity(context.Activity.CreateReply("AFTER"));
            }
        }
    }
}