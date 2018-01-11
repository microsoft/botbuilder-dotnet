using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Middleware;
using System;
using Microsoft.Bot.Connector;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("Russian Doll Middleware, Nested Middleware sets")]
    public class Middleware_ComposibilityTests
    {
        [TestMethod]
        public async Task NestedSet_OnReceive()
        {
            bool innerOnReceiveCalled = false;

            Middleware.MiddlewareSet inner = new Middleware.MiddlewareSet();
            inner.OnReceive(async (context, next) =>
               {
                   innerOnReceiveCalled = true;
                   await next(); 
               });

            Middleware.MiddlewareSet outer = new Middleware.MiddlewareSet();
            outer.Use(inner); 

            await outer.ReceiveActivity(null); 

            Assert.IsTrue(innerOnReceiveCalled, "Inner Middleware Receive was not called."); 
        }

        [TestMethod]
        public async Task NestedSet_OnContextCreated()
        {
            bool innerOnCreatedCalled = false;

            Middleware.MiddlewareSet inner = new Middleware.MiddlewareSet();
            inner.OnContextCreated(async (context, next) =>
            {
                innerOnCreatedCalled = true;
                await next();
            });

            Middleware.MiddlewareSet outer = new Middleware.MiddlewareSet();
            outer.Use(inner);

            await outer.ContextCreated(null);

            Assert.IsTrue(innerOnCreatedCalled, "Inner Middleware ContextCreated was not called.");
        }

        [TestMethod]
        public async Task NestedSet_OnPostActivity()
        {
            bool innerOnPostCalled = false;

            Middleware.MiddlewareSet inner = new Middleware.MiddlewareSet();

            inner.OnPostActivity(async (context, activities, next) =>
            {
                innerOnPostCalled = true;
                await next();
            });

            Middleware.MiddlewareSet outer = new Middleware.MiddlewareSet();
            outer.Use(inner);

            await outer.PostActivity(null, new List<IActivity>());

            Assert.IsTrue(innerOnPostCalled, "Inner Middleware PostActivity was not called.");
        }

        [TestMethod]
        public async Task NestedSet_AllIsRun()
        {
            bool innerOnPostCalled = false;
            bool innerOnReceiveCalled = false;
            bool innerOnCreatedCalled = false;
            string replyMessage = Guid.NewGuid().ToString();

            Middleware.MiddlewareSet inner = new Middleware.MiddlewareSet();
            inner.OnPostActivity(async (context, activities, next) =>
            {
                Assert.IsTrue(activities.Count == 1, "incorrect activity count");
                Assert.IsTrue(activities[0].AsMessageActivity().Text == replyMessage, "unexpected message"); 

                innerOnPostCalled = true;
                await next();
            });

            inner.OnReceive(async (context, next) =>
            {
                context.Responses.Add(MessageFactory.Text(replyMessage));                
                innerOnReceiveCalled = true;
                await next();
            });

            inner.OnContextCreated(async (context, next) =>
            {
                innerOnCreatedCalled = true;
                await next();
            });

            Middleware.MiddlewareSet outer = new Middleware.MiddlewareSet();
            outer.Use(inner);

            IBotContext c = TestUtilities.CreateEmptyContext();
            await outer.ContextCreated(c);
            await outer.ReceiveActivity(c);
            await outer.PostActivity(c, c.Responses);

            Assert.IsTrue(innerOnReceiveCalled, "Inner Middleware Receive Activity was not called.");
            Assert.IsTrue(innerOnCreatedCalled, "Inner Middleware Create Context was not called.");
            Assert.IsTrue(innerOnPostCalled, "Inner Middleware PostActivity was not called.");
        }
    }
}