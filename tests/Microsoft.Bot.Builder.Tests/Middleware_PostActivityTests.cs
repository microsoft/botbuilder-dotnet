// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("Russian Doll Middleware, Post Activity Pipeline Tests")]
    public class Middleware_PostActivityTests
    {
        [TestMethod]
        public async Task NoMiddleware()
        {
            Middleware.MiddlewareSet m = new Middleware.MiddlewareSet();
            // No middleware. Should not explode. 
            await m.PostActivity(null, new List<IActivity>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ThrowOnNullMiddlware()
        {
            Middleware.MiddlewareSet m = new Middleware.MiddlewareSet();
            m.Use((Middleware.IPostActivity)null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task BubbleUncaughtException()
        {
            Middleware.MiddlewareSet m = new Middleware.MiddlewareSet();
            m.OnPostActivity(async (context, activities, next) =>
            {
                throw new InvalidOperationException("test");
            });

            await m.PostActivity(null, new List<IActivity>());
            Assert.Fail("Should never have gotten here");
        }

        [TestMethod]
        public async Task OneMiddlewareItem()
        {
            WasCalledMiddlware simple = new WasCalledMiddlware();

            Middleware.MiddlewareSet m = new Middleware.MiddlewareSet();
            m.Use(simple);

            Assert.IsFalse(simple.Called);
            await m.PostActivity(null, new List<IActivity>()); 
            Assert.IsTrue(simple.Called);
        }

        [TestMethod]
        public async Task TwoMiddlewareItems()
        {
            WasCalledMiddlware one = new WasCalledMiddlware();
            WasCalledMiddlware two = new WasCalledMiddlware();

            Middleware.MiddlewareSet m = new Middleware.MiddlewareSet();
            m.Use(one);
            m.Use(two);

            await m.PostActivity(null, new List<IActivity>());
            Assert.IsTrue(one.Called);
            Assert.IsTrue(two.Called);
        }

        [TestMethod]
        public async Task TwoMiddlewareItemsInOrder()
        {
            bool called1 = false;
            bool called2 = false;

            CallMeMiddlware one = new CallMeMiddlware((activities) =>
            {
                Assert.IsFalse(called2, "Second Middleware was called");
                called1 = true;
            });

            CallMeMiddlware two = new CallMeMiddlware((activities) =>
            {
                Assert.IsTrue(called1, "First Middleware was not called");
                called2 = true;
            });

            Middleware.MiddlewareSet m = new Middleware.MiddlewareSet();
            m.Use(one);
            m.Use(two);

            await m.PostActivity(null, new List<IActivity>());
            Assert.IsTrue(called1);
            Assert.IsTrue(called2);
        }

        [TestMethod]
        public async Task ActivityListPassedThrough()
        {
            bool didRun = false;
            string message = Guid.NewGuid().ToString(); 

            Middleware.MiddlewareSet m = new Middleware.MiddlewareSet();
            m.OnPostActivity(async (context, activities, next) =>
            {
                Assert.IsTrue(activities.Count == 1);
                Assert.IsTrue(activities[0].AsMessageActivity().Text == message); 

                didRun = true;
                await next();
            });

            Assert.IsFalse(didRun);
            await m.PostActivity(null, new List<IActivity> { MessageFactory.Text(message) });
            Assert.IsTrue(didRun);
        }

        [TestMethod]
        public async Task ManiuplateActivityList()
        {
            bool didRun1 = false;
            bool didRun2 = false;
            bool didRun3 = false;

            string message1 = Guid.NewGuid().ToString();
            string message2 = Guid.NewGuid().ToString();
            string message3 = Guid.NewGuid().ToString();

            Middleware.MiddlewareSet m = new Middleware.MiddlewareSet();
            m.OnPostActivity(async (context, activities, next) =>
            {
                Assert.IsTrue(activities.Count == 1);
                Assert.IsTrue(activities[0].AsMessageActivity().Text == message1);
                activities.Add(MessageFactory.Text(message2));
                didRun1 = true;

                await next();

                // After the next 2 middleware elements fire, the list has been changed.
                // As the "parent" this method can see those changes. 
                Assert.IsTrue(activities.Count == 2);
                Assert.IsTrue(activities[0].AsMessageActivity().Text == message3);
                Assert.IsTrue(activities[1].AsMessageActivity().Text == message2);
            });
            m.OnPostActivity(async (context, activities, next) =>
            {
                Assert.IsTrue(activities.Count == 2);
                Assert.IsTrue(activities[0].AsMessageActivity().Text == message1);
                Assert.IsTrue(activities[1].AsMessageActivity().Text == message2);

                // change the text of the 1st message and verify it's passed through
                activities[0].AsMessageActivity().Text = message3;
                didRun2 = true;

                await next();

                // After the "3" middleware runs, it's manipulated the messages. 
                // Verify this middleware can see them. 
                Assert.IsTrue(activities.Count == 2);
                Assert.IsTrue(activities[0].AsMessageActivity().Text == message3);
                Assert.IsTrue(activities[1].AsMessageActivity().Text == message2);
            });

            m.OnPostActivity(async (context, activities, next) =>
            {
                Assert.IsTrue(activities.Count == 2);
                
                // "Message 0" was previously maniupulated to have it's text changed. 
                Assert.IsTrue(activities[0].AsMessageActivity().Text == message3);
                Assert.IsTrue(activities[1].AsMessageActivity().Text == message2);
                didRun3 = true;
                await next();
            });

            await m.PostActivity(null, new List<IActivity> { MessageFactory.Text(message1) });
            Assert.IsTrue(didRun1);
            Assert.IsTrue(didRun2);
            Assert.IsTrue(didRun3);
        }


        public class WasCalledMiddlware : Middleware.IPostActivity
        {
            public bool Called { get; set; } = false;

            public Task PostActivity(IBotContext context, IList<IActivity> activities, Middleware.MiddlewareSet.NextDelegate next)
            {
                Called = true;
                return next();
            }            
        }

        public class CallMeMiddlware : Middleware.IPostActivity
        {
            private readonly Action<IList<IActivity>> _callMe;
            public CallMeMiddlware(Action<IList<IActivity>> callMe)
            {
                _callMe = callMe;
            }

            public Task PostActivity(IBotContext context, IList<IActivity> activities, Middleware.MiddlewareSet.NextDelegate next)
            {
                _callMe(activities);
                return next();
            }
        }
    }
}
