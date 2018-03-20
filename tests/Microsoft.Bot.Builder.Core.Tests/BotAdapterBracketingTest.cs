using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Core.Tests
{
    [TestClass]
    [TestCategory("Middleware")]
    public class BotAdapterBracketingTest
    {

        /// <summary>
        /// Developer authored Middleware that looks like this:
        /// public async Task ReceiveActivity(IBotContext context, 
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
            TestAdapter adapter = new TestAdapter()
                .Use(new BeforeAFterMiddlware());

            async Task Echo(IBotContext ctx)
            {
                string toEcho = "ECHO:" + ctx.Request.AsMessageActivity().Text;
                await ctx.SendActivity(ctx.Request.CreateReply(toEcho)); 
            }

            await new TestFlow(adapter, Echo)
                .Send("test")
                .AssertReply("BEFORE")
                .AssertReply("ECHO:test")
                .AssertReply("AFTER")
                .StartTest();
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
            string uniqueId = Guid.NewGuid().ToString();

            TestAdapter adapter = new TestAdapter()
                .Use(new CatchExceptionMiddleware());

            async Task EchoWithException(IBotContext ctx)
            {
                string toEcho = "ECHO:" + ctx.Request.AsMessageActivity().Text;
                await ctx.SendActivity(ctx.Request.CreateReply(toEcho));
                throw new Exception(uniqueId);
            }

            await new TestFlow(adapter, EchoWithException)
                .Send("test")
                .AssertReply("BEFORE")
                .AssertReply("ECHO:test")
                .AssertReply("CAUGHT:" + uniqueId)
                .AssertReply("AFTER")
                .StartTest();
        }

        public class CatchExceptionMiddleware : IMiddleware
        {
            public async Task OnProcessRequest(IBotContext context, MiddlewareSet.NextDelegate next)
            {
                await context.SendActivity(context.Request.CreateReply("BEFORE"));
                try
                {
                    await next();
                }
                catch (Exception ex)
                {
                    await context.SendActivity(context.Request.CreateReply("CAUGHT:" + ex.Message));                    
                }

                await context.SendActivity(context.Request.CreateReply("AFTER"));
            }            
        }

        public class BeforeAFterMiddlware : IMiddleware
        {
            public async Task OnProcessRequest(IBotContext context, MiddlewareSet.NextDelegate next)
            {
                await context.SendActivity(context.Request.CreateReply("BEFORE"));
                await next();
                await context.SendActivity(context.Request.CreateReply("AFTER"));
            }
        }
    }
}