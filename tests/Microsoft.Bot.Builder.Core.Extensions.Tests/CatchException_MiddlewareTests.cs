using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Core.Extensions.Tests
{
    [TestClass]
    public class CatchException_MiddlewareTests
    {
        [TestMethod]
        [TestCategory("Middleware")]
        public async Task CatchException_TestMiddleware()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new CatchExceptionMiddleware((context, exception) =>
                {
                    context.SendActivity(context.Request.CreateReply("Sorry, something went wrong"));
                    return Task.CompletedTask;
                }));


            await new TestFlow(adapter, (context) =>
                {
                    if (context.Request.AsMessageActivity().Text == "foo")
                    {
                        context.SendActivity(context.Request.CreateReply(context.Request.AsMessageActivity().Text));
                    }

                    if (context.Request.AsMessageActivity().Text == "error")
                    {
                        throw new Exception();
                    }

                    return Task.CompletedTask;
                })
                .Send("foo")
                .AssertReply("foo", "passthrough")
                .Send("error")
                .AssertReply("Sorry, something went wrong")
                .StartTest();
        }
    }
}
