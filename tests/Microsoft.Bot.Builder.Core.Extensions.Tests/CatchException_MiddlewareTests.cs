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
        public async Task CatchException_TestMiddleware_TestStackedErrorMiddleware()
        {
            TestAdapter adapter = new TestAdapter()
                // Add middleware to catch general exceptions
                .Use(new CatchExceptionMiddleware<Exception>((context, exception) =>
                {
                    context.SendActivity(context.Request.CreateReply(exception.Message));
                    return Task.CompletedTask;
                }))
                // Add middleware to catch NullReferenceExceptions before throwing up to the general exception instance
                .Use(new CatchExceptionMiddleware<NullReferenceException>((context, exception) =>
                {
                    context.SendActivity(context.Request.CreateReply("Sorry - Null Reference Exception"));
                    return Task.CompletedTask;
                }));


            await new TestFlow(adapter, (context) =>
                {
                    if (context.Request.AsMessageActivity().Text == "foo")
                    {
                        context.SendActivity(context.Request.CreateReply(context.Request.AsMessageActivity().Text));
                    }

                    if (context.Request.AsMessageActivity().Text == "NotImplementedException")
                    {
                        throw new NotImplementedException("Test");
                    }

                    return Task.CompletedTask;
                })
                .Send("foo")
                .AssertReply("foo", "passthrough")
                .Send("NotImplementedException")
                .AssertReply("Test")
                .StartTest();
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task CatchException_TestMiddleware_SpecificExceptionType()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new CatchExceptionMiddleware<Exception>((context, exception) =>
                {
                    context.SendActivity(context.Request.CreateReply("Generic Exception Caught"));
                    return Task.CompletedTask;
                }))
                .Use(new CatchExceptionMiddleware<NullReferenceException>((context, exception) =>
                {
                    context.SendActivity(context.Request.CreateReply(exception.Message));
                    return Task.CompletedTask;
                }));


            await new TestFlow(adapter, (context) =>
                {
                    if (context.Request.AsMessageActivity().Text == "foo")
                    {
                        context.SendActivity(context.Request.CreateReply(context.Request.AsMessageActivity().Text));
                    }

                    if (context.Request.AsMessageActivity().Text == "NullReferenceException")
                    {
                        throw new NullReferenceException("Test");
                    }

                    return Task.CompletedTask;
                })
                .Send("foo")
                .AssertReply("foo", "passthrough")
                .Send("NullReferenceException")
                .AssertReply("Test")
                .StartTest();
        }
    }
}
