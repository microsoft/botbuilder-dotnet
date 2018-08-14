// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("Russian Doll Middleware, Nested Middleware sets")]
    public class MiddlewareSetTests
    {
        [TestMethod]
        public async Task NestedSet_OnReceive()
        {
            bool innerOnReceiveCalled = false;

            MiddlewareSet inner = new MiddlewareSet();
            inner.Use(new AnonymousReceiveMiddleware(async (context, next, cancellationToken) =>
            {
                innerOnReceiveCalled = true;
                await next(cancellationToken);
            }));

            MiddlewareSet outer = new MiddlewareSet();
            outer.Use(inner);

            await outer.ReceiveActivityAsync(null, default(CancellationToken));

            Assert.IsTrue(innerOnReceiveCalled, "Inner Middleware Receive was not called.");
        }

        [TestMethod]
        public async Task NoMiddleware()
        {
            MiddlewareSet m = new MiddlewareSet();

            // No middleware. Should not explode. 
            await m.ReceiveActivityAsync(null, default(CancellationToken));
        }

        [TestMethod]
        public async Task NoMiddlewareWithDelegate()
        {
            var m = new MiddlewareSet();
            bool wasCalled = false;
            Task CallMe(ITurnContext context, CancellationToken cancellationToken)
            {
                wasCalled = true;
                return Task.CompletedTask;
            }
            // No middleware. Should not explode. 
            await m.ReceiveActivityWithStatusAsync(null, CallMe, default(CancellationToken));
            Assert.IsTrue(wasCalled, "Delegate was not called");
        }

        [TestMethod]
        public async Task OneMiddlewareItem()
        {
            var simple = new WasCalledMiddlware();

            bool wasCalled = false;
            Task CallMe(ITurnContext context, CancellationToken cancellationToken)
            {
                wasCalled = true;
                return Task.CompletedTask;
            }

            MiddlewareSet m = new MiddlewareSet();
            m.Use(simple);

            Assert.IsFalse(simple.Called);
            await m.ReceiveActivityWithStatusAsync(null, CallMe, default(CancellationToken));
            Assert.IsTrue(simple.Called);
            Assert.IsTrue(wasCalled, "Delegate was not called"); 
        }

        [TestMethod]
        public async Task OneMiddlewareItemWithDelegate()
        {
            WasCalledMiddlware simple = new WasCalledMiddlware();

            MiddlewareSet m = new MiddlewareSet();
            m.Use(simple);

            Assert.IsFalse(simple.Called);
            await m.ReceiveActivityAsync(null, default(CancellationToken));
            Assert.IsTrue(simple.Called);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task BubbleUncaughtException()
        {
            MiddlewareSet m = new MiddlewareSet();
            m.Use(new AnonymousReceiveMiddleware((context, next, cancellationToken) =>
            {
                throw new InvalidOperationException("test");
            }));

            await m.ReceiveActivityAsync(null, default(CancellationToken));
            Assert.Fail("Should never have gotten here");
        }

        [TestMethod]
        public async Task TwoMiddlewareItems()
        {
            WasCalledMiddlware one = new WasCalledMiddlware();
            WasCalledMiddlware two = new WasCalledMiddlware();

            MiddlewareSet m = new MiddlewareSet();
            m.Use(one);
            m.Use(two);

            await m.ReceiveActivityAsync(null, default(CancellationToken));
            Assert.IsTrue(one.Called);
            Assert.IsTrue(two.Called);
        }

        [TestMethod]
        public async Task TwoMiddlewareItemsWithDelegate()
        {
            WasCalledMiddlware one = new WasCalledMiddlware();
            WasCalledMiddlware two = new WasCalledMiddlware();

            int called = 0;
            Task CallMe(ITurnContext context, CancellationToken cancellationToken)
            {
                called++;
                return Task.CompletedTask;
            }

            MiddlewareSet m = new MiddlewareSet();
            m.Use(one);
            m.Use(two);

            await m.ReceiveActivityWithStatusAsync(null, CallMe, default(CancellationToken));
            Assert.IsTrue(one.Called);
            Assert.IsTrue(two.Called);
            Assert.IsTrue(called == 1, "Incorrect number of calls to Delegate"); 
        }

        [TestMethod]
        public async Task TwoMiddlewareItemsInOrder()
        {
            bool called1 = false;
            bool called2 = false;

            CallMeMiddlware one = new CallMeMiddlware(() =>
            {
                Assert.IsFalse(called2, "Second Middleware was called");
                called1 = true;
            });

            CallMeMiddlware two = new CallMeMiddlware(() =>
            {
                Assert.IsTrue(called1, "First Middleware was not called");
                called2 = true;
            });

            MiddlewareSet m = new MiddlewareSet();
            m.Use(one);
            m.Use(two);

            await m.ReceiveActivityAsync(null, default(CancellationToken));
            Assert.IsTrue(called1);
            Assert.IsTrue(called2);
        }

        [TestMethod]
        public async Task Status_OneMiddlewareRan()
        {
            bool called1 = false;

            var one = new CallMeMiddlware(() => { called1 = true; });

            var m = new MiddlewareSet();
            m.Use(one);

            // The middlware in this pipeline calls next(), so the delegate should be called
            bool didAllRun = false;
            await m.ReceiveActivityWithStatusAsync(null, (ctx, cancellationToken) =>
            {
                didAllRun = true;
                return Task.CompletedTask;
            },
            default(CancellationToken));

            Assert.IsTrue(called1);
            Assert.IsTrue(didAllRun);
        }

        [TestMethod]
        public async Task Status_RunAtEndEmptyPipeline()
        {
            var m = new MiddlewareSet();
            bool didAllRun = false;

            // This middlware pipeline has no entries. This should result in
            // the status being TRUE. 
            await m.ReceiveActivityWithStatusAsync(null, (ctx, cancellationToken) =>
            {
                didAllRun = true;
                return Task.CompletedTask;
            },
            default(CancellationToken));
            Assert.IsTrue(didAllRun);
        }

        [TestMethod]
        public async Task Status_TwoItemsOneDoesNotCallNext()
        {
            bool called1 = false;
            bool called2 = false;

            var one = new CallMeMiddlware(() =>
            {
                Assert.IsFalse(called2, "Second Middleware was called");
                called1 = true;
            });

            var two = new DoNotCallNextMiddleware(() =>
            {
                Assert.IsTrue(called1, "First Middleware was not called");
                called2 = true;
            });

            var m = new MiddlewareSet();
            m.Use(one);
            m.Use(two);

            bool didAllRun = false;
            await m.ReceiveActivityWithStatusAsync(null, (ctx, cancellationToken) =>
            {
                didAllRun = true;
                return Task.CompletedTask;
            },
            default(CancellationToken));
            Assert.IsTrue(called1);
            Assert.IsTrue(called2);

            // The 2nd middleware did not call next, so the "final" action should not have run. 
            Assert.IsFalse(didAllRun);
        }

        [TestMethod]
        public async Task Status_OneEntryThatDoesNotCallNext()
        {
            bool called1 = false;

            var one = new DoNotCallNextMiddleware(() => { called1 = true; });

            var m = new MiddlewareSet();
            m.Use(one);

            // The middlware in this pipeline DOES NOT call next(), so this must not be called 
            bool didAllRun = false;
            await m.ReceiveActivityWithStatusAsync(null, (ctx, cancellationToken) =>
            {
                didAllRun = true;
                return Task.CompletedTask;
            },
            default(CancellationToken));

            Assert.IsTrue(called1);

            // Our "Final" action MUST NOT have been called, as the Middlware Pipeline
            // didn't complete. 
            Assert.IsFalse(didAllRun);
        }

        [TestMethod]
        public async Task AnonymousMiddleware()
        {
            bool didRun = false;

            var m = new MiddlewareSet();
            m.Use(new AnonymousReceiveMiddleware(async (context, next, cancellationToken) =>
            {
                didRun = true;
                await next(cancellationToken);
            }));

            Assert.IsFalse(didRun);
            await m.ReceiveActivityAsync(null, default(CancellationToken));
            Assert.IsTrue(didRun);
        }

        [TestMethod]
        public async Task TwoAnonymousMiddleware()
        {
            bool didRun1 = false;
            bool didRun2 = false;

            MiddlewareSet m = new MiddlewareSet();
            m.Use(new AnonymousReceiveMiddleware(async (context, next, cancellationToken) =>
            {
                didRun1 = true;
                await next(cancellationToken);
            }));
            m.Use(new AnonymousReceiveMiddleware(async (context, next, cancellationToken) =>
            {
                didRun2 = true;
                await next(cancellationToken);
            }));

            await m.ReceiveActivityAsync(null, default(CancellationToken));
            Assert.IsTrue(didRun1);
            Assert.IsTrue(didRun2);
        }

        [TestMethod]
        public async Task TwoAnonymousMiddlewareInOrder()
        {
            bool didRun1 = false;
            bool didRun2 = false;

            MiddlewareSet m = new MiddlewareSet();
            m.Use(new AnonymousReceiveMiddleware(async (context, next, cancellationToken) =>
            {
                Assert.IsFalse(didRun2, "Looks like the 2nd one has already run");
                didRun1 = true;
                await next(cancellationToken);
            }));
            m.Use(new AnonymousReceiveMiddleware(async (context, next, cancellationToken) =>
            {
                Assert.IsTrue(didRun1, "Looks like the 1nd one has not yet run");
                didRun2 = true;
                await next(cancellationToken);
            }));

            await m.ReceiveActivityAsync(null, default(CancellationToken));
            Assert.IsTrue(didRun1);
            Assert.IsTrue(didRun2);
        }

        [TestMethod]
        public async Task MixedMiddlewareInOrderAnonymousFirst()
        {
            bool didRun1 = false;
            bool didRun2 = false;

            MiddlewareSet m = new MiddlewareSet();

            m.Use(new AnonymousReceiveMiddleware(async (context, next, cancellationToken) =>
            {
                Assert.IsFalse(didRun1, "First middleware already ran");
                Assert.IsFalse(didRun2, "Looks like the second middleware was already run");
                didRun1 = true;
                await next(cancellationToken);
                Assert.IsTrue(didRun2, "Second middleware should have completed running");
            }));

            m.Use(
                new CallMeMiddlware(() =>
                {
                    Assert.IsTrue(didRun1, "First middleware should have already been called");
                    Assert.IsFalse(didRun2, "Second middleware should not have been invoked yet");
                    didRun2 = true;
                }));

            await m.ReceiveActivityAsync(null, default(CancellationToken));
            Assert.IsTrue(didRun1);
            Assert.IsTrue(didRun2);
        }

        [TestMethod]
        public async Task MixedMiddlewareInOrderAnonymousLast()
        {
            bool didRun1 = false;
            bool didRun2 = false;

            MiddlewareSet m = new MiddlewareSet();

            m.Use(
                new CallMeMiddlware(() =>
                {
                    Assert.IsFalse(didRun1, "First middleware should not have been called yet");
                    Assert.IsFalse(didRun2, "Second Middleware should not have been called yet");
                    didRun1 = true;
                }));

            m.Use(new AnonymousReceiveMiddleware(async (context, next, cancellationToken) =>
            {
                Assert.IsTrue(didRun1, "First middleware has not been run yet");
                didRun2 = true;
                await next(cancellationToken);

            }));

            await m.ReceiveActivityAsync(null, default(CancellationToken));
            Assert.IsTrue(didRun1);
            Assert.IsTrue(didRun2);
        }

        [TestMethod]
        public async Task RunCodeBeforeAndAfter()
        {
            bool didRun1 = false;
            bool codeafter2run = false;
            bool didRun2 = false;

            MiddlewareSet m = new MiddlewareSet();

            m.Use(new AnonymousReceiveMiddleware(async (context, next, cancellationToken) =>
            {
                Assert.IsFalse(didRun1, "Looks like the 1st middleware has already run");
                didRun1 = true;
                await next(cancellationToken);
                Assert.IsTrue(didRun1, "The 2nd middleware should have run now.");
                codeafter2run = true;
            }));

            m.Use(new AnonymousReceiveMiddleware(async (context, next, cancellationToken) =>
            {
                Assert.IsTrue(didRun1, "Looks like the 1st middleware has not been run");
                Assert.IsFalse(codeafter2run, "The code that runs after middleware 2 is complete has already run.");
                didRun2 = true;
                await next(cancellationToken);
            }));

            await m.ReceiveActivityAsync(null, default(CancellationToken));
            Assert.IsTrue(didRun1);
            Assert.IsTrue(didRun2);
            Assert.IsTrue(codeafter2run);
        }

        [TestMethod]
        public async Task CatchAnExceptionViaMiddlware()
        {
            var m = new MiddlewareSet();
            bool caughtException = false;

            m.Use(new AnonymousReceiveMiddleware(async (context, next, cancellationToken) =>
            {
                try
                {
                    await next(cancellationToken);
                    Assert.Fail("Should not get here");
                }
                catch (Exception ex)
                {
                    Assert.IsTrue(ex.Message == "test");
                    caughtException = true;
                }
            }));

            m.Use(new AnonymousReceiveMiddleware((context, next, cancellationToken) =>
            {
                throw new Exception("test");
            }));

            await m.ReceiveActivityAsync(null, default(CancellationToken));
            Assert.IsTrue(caughtException);
        }

        public class WasCalledMiddlware : IMiddleware
        {
            public bool Called { get; set; } = false;

            public Task OnTurnAsync(ITurnContext context, NextDelegate next, CancellationToken cancellationToken)
            {
                Called = true;
                return next(cancellationToken);
            }
        }

        public class DoNotCallNextMiddleware : IMiddleware
        {
            private readonly Action _callMe;
            public DoNotCallNextMiddleware(Action callMe)
            {
                _callMe = callMe;
            }
            public Task OnTurnAsync(ITurnContext context, NextDelegate next, CancellationToken cancellationToken)
            {
                _callMe();
                // DO NOT call NEXT
                return Task.CompletedTask;
            }
        }

        public class CallMeMiddlware : IMiddleware
        {
            private readonly Action _callMe;
            public CallMeMiddlware(Action callMe)
            {
                _callMe = callMe;
            }
            public Task OnTurnAsync(ITurnContext context, NextDelegate next, CancellationToken cancellationToken)
            {
                _callMe();
                return next(cancellationToken);
            }

        }
    }
}
