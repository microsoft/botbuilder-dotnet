// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

            MiddlewareSet inner = new MiddlewareSet();
            inner.Use(new AnonymousReceiveMiddleware(async (context, next) =>
            {
                innerOnReceiveCalled = true;
                await next();
            }));

            MiddlewareSet outer = new MiddlewareSet();
            outer.Use(inner);

            await outer.ReceiveActivity(null);

            Assert.IsTrue(innerOnReceiveCalled, "Inner Middleware Receive was not called.");
        }

        [TestMethod]
        public async Task NestedSet_OnContextCreated()
        {
            bool innerOnCreatedCalled = false;

            MiddlewareSet inner = new MiddlewareSet();
            inner.Use(new AnonymousContextCreatedMiddleware(async (context, next) =>
            {
                innerOnCreatedCalled = true;
                await next();
            }));

            MiddlewareSet outer = new MiddlewareSet();
            outer.Use(inner);

            await outer.ContextCreated(null);

            Assert.IsTrue(innerOnCreatedCalled, "Inner Middleware ContextCreated was not called.");
        }

        [TestMethod]
        public async Task NestedSet_OnPostActivity()
        {
            bool innerOnSendCalled = false;

            MiddlewareSet inner = new MiddlewareSet();

            inner.Use(new AnonymousSendActivityMiddleware(async (context, activities, next) =>
           {
               innerOnSendCalled = true;
               await next();
           }));

            MiddlewareSet outer = new MiddlewareSet();
            outer.Use(inner);

            await outer.SendActivity(null, new List<Activity>());

            Assert.IsTrue(innerOnSendCalled, "Inner Middleware SendActivity was not called.");
        }

        [TestMethod]
        public async Task NestedSet_AllIsRun()
        {
            bool innerOnSendCalled = false;
            bool innerOnReceiveCalled = false;
            bool innerOnCreatedCalled = false;
            string replyMessage = Guid.NewGuid().ToString();

            MiddlewareSet inner = new MiddlewareSet();
            inner.Use(new AnonymousSendActivityMiddleware( async (context, activities, next) =>
            {
                Assert.IsTrue(activities.Count == 1, "incorrect activity count");
                Assert.IsTrue(activities[0].AsMessageActivity().Text == replyMessage, "unexpected message");

                innerOnSendCalled = true;
                await next();
            }));

            inner.Use( new AnonymousReceiveMiddleware(async (context, next) =>
            {
                context.Responses.Add(MessageFactory.Text(replyMessage));
                innerOnReceiveCalled = true;
                await next();
            }));

            inner.Use(new AnonymousContextCreatedMiddleware(async (context, next) =>
            {
                innerOnCreatedCalled = true;
                await next();
            }));

            Middleware.MiddlewareSet outer = new Middleware.MiddlewareSet();
            outer.Use(inner);

            IBotContext c = TestUtilities.CreateEmptyContext();
            await outer.ContextCreated(c);
            await outer.ReceiveActivity(c);
            await outer.SendActivity(c, c.Responses);

            Assert.IsTrue(innerOnReceiveCalled, "Inner Middleware Receive Activity was not called.");
            Assert.IsTrue(innerOnCreatedCalled, "Inner Middleware Create Context was not called.");
            Assert.IsTrue(innerOnSendCalled, "Inner Middleware SendActivity was not called.");
        }
    }
}