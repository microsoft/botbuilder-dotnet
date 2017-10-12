using Microsoft.Bot.Builder.Prague;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    public class RouterTests
    {
        [TestMethod]
        public async Task NullRouterTests()
        {
            NullRouter nullRouter = new NullRouter();
            BotContext bc = TestUtilities.CreateEmptyContext();
            Route r = await nullRouter.GetRoute(bc);
            Assert.IsNull(r, "Null Route Expected. Found a route.");
        }

        [TestMethod]
        public async Task SimpleRouteTest()
        {
            SimpleTestRouter tr = new SimpleTestRouter();
            Route r = await tr.GetRoute(TestUtilities.CreateEmptyContext());
            Assert.IsNotNull(r, "Expected a Route. Did not get one.");

            Assert.IsFalse(tr.Routed, "Expected Routed to initially be false");
            await r.Action();
            Assert.IsTrue(tr.Routed, "Expected Routed to initially be true");
        }

        class SimpleTestRouter : IRouter
        {
            public bool Routed { get; set; } = false;

            public async Task<Route> GetRoute(IBotContext context)
            {
                return new Route(async () => Routed = true);
            }
        }

        [TestMethod]
        public async Task NullMessageRouter()
        {
            //Should not explode. :)
            await RoutingUtilities.RouteMessage(
                RoutingUtilities.NullRouter,
                TestUtilities.CreateEmptyContext());
        }

        [TestMethod]
        public async Task RouteToTestRouter()
        {
            SimpleTestRouter r = new SimpleTestRouter();

            Assert.IsFalse(r.Routed, "Expecting Routed to be FALSE");
            await RoutingUtilities.RouteMessage(
                r,
                TestUtilities.CreateEmptyContext());
            Assert.IsTrue(r.Routed, "Expecting Routed to be TRUE");
        }
        [TestMethod]
        public async Task SimpleRouterRoute()
        {
            bool routed = false;

            await RoutingUtilities.RouteMessage(
                new SimpleRouter(
                    () => routed = true),
                TestUtilities.CreateEmptyContext());

            Assert.IsTrue(routed, "Expecting Routed to be TRUE");
        }

        [TestMethod]
        public async Task SimpleRouterRouteWithContext()
        {
            bool routed = false;
            IBotContext bc = TestUtilities.CreateEmptyContext();
            bc.State["test"] = true;

            await RoutingUtilities.RouteMessage(
                new SimpleRouter(
                    () => routed = (bc.State["test"] == true)),
                TestUtilities.CreateEmptyContext());

            Assert.IsTrue(routed, "Expecting Routed to be TRUE");
        }

        [TestMethod]
        public async Task AnonymousRouterTest()
        {
            bool routed = false;

            await RoutingUtilities.RouteMessage(
                new AnonymousRouter(async (context) =>
                            new Route(async () => routed = true)),
                TestUtilities.CreateEmptyContext());

            Assert.IsTrue(routed, "expecting Routed to be TRUE");
        }

        [TestMethod]
        public async Task HandlerToRouterConversionTest()
        {
            bool routed = false;

            IHandler handler = SimpleHandler.Create(() => routed = true);

            await RoutingUtilities.RouteMessage(
                handler.AsRouter(),
                TestUtilities.CreateEmptyContext());

            Assert.IsTrue(routed, "expecting Routed to be TRUE");
        }
        [TestMethod]
        [TestCategory("First Router")]
        public async Task First_NullRoutingTests()
        {
            // Should complete and not explode.
            await RoutingUtilities.RouteMessage(
                new FirstRouter(),
                TestUtilities.CreateEmptyContext());

            await RoutingUtilities.RouteMessage(
                new FirstRouter(
                    RoutingUtilities.NullRouter),
                TestUtilities.CreateEmptyContext());

            await RoutingUtilities.RouteMessage(
                new FirstRouter()
                    .Add(RoutingUtilities.NullRouter),
                TestUtilities.CreateEmptyContext());

            await RoutingUtilities.RouteMessage(
                new FirstRouter()
                    .Add(RoutingUtilities.NullRouter)
                    .Add(RoutingUtilities.NullRouter)
                    .Add(RoutingUtilities.NullRouter),
                TestUtilities.CreateEmptyContext());

        }

        [TestMethod]
        [TestCategory("First Router")]
        public async Task First_RoutingTests()
        {
            bool routed = false;

            await RoutingUtilities.RouteMessage(
                new FirstRouter(
                    SimpleRouter.Create(() => routed = true)),
                TestUtilities.CreateEmptyContext());

            Assert.IsTrue(routed, "expecting Routed to be TRUE");

            routed = false;
            await RoutingUtilities.RouteMessage(
                new FirstRouter()
                    .Add(SimpleRouter.Create(() => routed = true)),
                TestUtilities.CreateEmptyContext());
            Assert.IsTrue(routed, "expecting Routed to be TRUE");
        }

        [TestMethod]
        [TestCategory("First Router")]
        public async Task First_RoutingTests_RoutePastNull()
        {
            bool routed = false;

            await RoutingUtilities.RouteMessage(
                new FirstRouter()
                    .Add(new NullRouter())
                    .Add((IHandler)null)
                    .Add((IRouter)null)
                    .Add(SimpleRouter.Create(() => routed = true)),
                TestUtilities.CreateEmptyContext());

            Assert.IsTrue(routed, "expecting Routed to be TRUE");
        }

        [TestMethod]
        [TestCategory("First Router")]
        public async Task First_DifferentRouterTypes()
        {
            bool routed = false;

            IRouter simpleViaStatic = SimpleRouter.Create(() => routed = true);
            IRouter simpleViaCtor = new SimpleRouter(() => routed = true);
            IRouter nullRouterviaStatic = RoutingUtilities.NullRouter;
            IRouter nullRouterviaCtor = new NullRouter();
            IRouter anonymousRouter = new AnonymousRouter(
                       async (context) =>
                             new Route(
                                async () => routed = true));
            IRouter testRouter = new SimpleTestRouter();
        }

        [TestMethod]
        [TestCategory("Best Router")]
        public async Task Best_NullRoutingTests()
        {
            // Should complete and not explode.
            await RoutingUtilities.RouteMessage(
                new BestRouter(),
                TestUtilities.CreateEmptyContext());

            await RoutingUtilities.RouteMessage(
                new BestRouter()
                    .Add((IHandler)null)
                    .Add((IRouter)null)
                    .Add(RoutingUtilities.NullRouter),
                TestUtilities.CreateEmptyContext());
        }

        [TestMethod]
        [TestCategory("Best Router")]
        public async Task Best_RouteToHandler()
        {
            bool routed = false;
            // Should complete and not explode.
            await RoutingUtilities.RouteMessage(
                new BestRouter()
                    .Add(new SimpleHandler(() => routed = true)),
                TestUtilities.CreateEmptyContext());

            Assert.IsTrue(routed, "Expected routed to be TRUE");
        }


        [TestMethod]
        [TestCategory("Best Router")]
        public async Task Best_RoutePastNullToHandler()
        {
            bool routed = false;
            // Should complete and not explode.
            await RoutingUtilities.RouteMessage(
                new BestRouter()
                    .Add(new NullRouter())
                    .Add((IRouter)null)
                    .Add((IHandler)null)
                    .Add(new SimpleHandler(() => routed = true)),
                TestUtilities.CreateEmptyContext());

            Assert.IsTrue(routed, "Expected routed to be TRUE");
        }

        /* This test exposes behavior in Best() that appears conceptually incorrect. A discussion
         * has been kicked off as to what the behavior of Best() should be when scoring trees are
         * scored in parallel. Until then, this test is commented out
         */
        //[TestMethod]
        //[TestCategory("Best Router")]
        //public async Task Best_StopAtFirstMaxScoreRouter()
        //{
        //    bool routed = false;

        //    // Should complete and not explode. Note, if the BestRouter passes the SimpleHandler, the Error Router
        //    // will throw and cause the test to fail. 
        //    await RoutingUtilities.RouteMessage(
        //        new BestRouter()
        //            .Add(new NullRouter())
        //            .Add(new SimpleHandler(() => routed = true))
        //            .Add(new ErrorRouter()),
        //        TestUtilities.CreateEmptyContext());

        //    Assert.IsTrue(routed, "Expected routed to be TRUE");
        //}

        [TestMethod]
        [TestCategory("Best Router")]
        [ExpectedException(typeof(InvalidOperationException), "No Exception found. Test failed.")]
        public async Task Best_FailOnExceptionDuringRouting()
        {
            // Should complete and not explode. Note, if the BestRouter passes the SimpleHandler, the Error Router
            // will throw and cause the test to fail. 
            await RoutingUtilities.RouteMessage(
                new BestRouter()
                    .Add(new ErrorRouter()),
                TestUtilities.CreateEmptyContext());
        }

        [TestMethod]
        [TestCategory("Best Router")]
        public async Task Best_HigherScoreFirst()
        {
            string whichRan = string.Empty;
            await RoutingUtilities.RouteMessage(
                new BestRouter()
                    .Add(new ScoredRouter(async () => whichRan = "0.5", 0.5))
                    .Add(new ScoredRouter(async () => whichRan = "0.4", 0.4)),
                TestUtilities.CreateEmptyContext());

            Assert.IsTrue(whichRan == "0.5", "Incorrect score was run");
        }

        [TestMethod]
        [TestCategory("Best Router")]
        public async Task Best_HigherScoreLast()
        {
            string whichRan = string.Empty;
            await RoutingUtilities.RouteMessage(
                new BestRouter()
                    .Add(new ScoredRouter(async () => whichRan = "0.4", 0.4))
                    .Add(new ScoredRouter(async () => whichRan = "0.5", 0.5)),
                TestUtilities.CreateEmptyContext());

            Assert.IsTrue(whichRan == "0.5", "Incorrect score was run");
        }

        [TestMethod]
        [TestCategory("Best Router")]
        public async Task Best_HigherScoreMiddle()
        {
            string whichRan = string.Empty;
            await RoutingUtilities.RouteMessage(
                new BestRouter()
                    .Add(new ScoredRouter(() => whichRan = "0.4", 0.4))
                    .Add(new ScoredRouter(() => whichRan = "0.9", 0.9))
                    .Add(new ScoredRouter(() => whichRan = "0.5", 0.5)),
                TestUtilities.CreateEmptyContext());

            Assert.IsTrue(whichRan == "0.9", "Incorrect score was run");
        }

        [TestMethod]
        [TestCategory("Best Router")]
        public async Task Best_TiedScores()
        {
            // Defined behavior is that if scores are tied, the first one wins. 
            string whichRan = string.Empty;
            await RoutingUtilities.RouteMessage(
                new BestRouter()
                    .Add(new ScoredRouter(() => whichRan = "first", 0.5))
                    .Add(new ScoredRouter(() => whichRan = "second", 0.5)),
                TestUtilities.CreateEmptyContext());

            Assert.IsTrue(whichRan == "first", "Incorrect score was run");
        }

        [TestMethod]
        [TestCategory("IfMatch Router")]
        [ExpectedException(typeof(ArgumentNullException), "Expected ArgumentNullException")]
        public async Task IfMatch_RequireParemters()
        {
            await RoutingUtilities.RouteMessage(
                new IfMatch((IfMatch.Condition)null, null),
                TestUtilities.CreateEmptyContext());
        }

        [TestMethod]
        [TestCategory("IfMatch Router")]
        public async Task IfMatch_ExecuteOnMatch()
        {
            bool result = false;
            await RoutingUtilities.RouteMessage(
                new IfMatch(
                    (context) => true,
                    new SimpleRouter(() => result = true)),
                TestUtilities.CreateEmptyContext());

            Assert.IsTrue(result == true, "Expected result to be TRUE");
        }

        [TestMethod]
        [TestCategory("IfMatch Router")]
        public async Task IfMatch_DoNotExecuteIfNoMatch()
        {
            bool result = false;
            await RoutingUtilities.RouteMessage(
                new IfMatch(
                    (context) => false,
                    new SimpleRouter(() => result = true)),
                TestUtilities.CreateEmptyContext());

            Assert.IsTrue(result == false, "Expected result to be FALSE. The Predicate incorrectly matched.");
        }

        [TestMethod]
        [TestCategory("IfMatch Router")]
        public async Task IfMatch_NoMatchNoElseClause()
        {
            // should complete and never emit on false predicate when 'else' router doesn't exist            
            await RoutingUtilities.RouteMessage(
                new IfMatch((context) => false, new ErrorRouter()),
                TestUtilities.CreateEmptyContext());

            // No Exception means the test has passed. 
        }

        [TestMethod]
        [TestCategory("IfMatch Router")]
        public async Task IfMatch_NoMatchRunsElseClauseNullRouter()
        {
            // should complete and never emit on false predicate when 'else' router doesn't route            
            await RoutingUtilities.RouteMessage(
                new IfMatch((context) => false, new ErrorRouter(), new NullRouter()),
                TestUtilities.CreateEmptyContext());

            // No Exception means the test has passed. 
        }

        [TestMethod]
        [TestCategory("IfMatch Router")]
        public async Task IfMatch_NoMatchRunsElseClause()
        {
            bool result = false;
            await RoutingUtilities.RouteMessage(
                new IfMatch((context) => false, new ErrorRouter(), new SimpleRouter(() => result = true)),
                TestUtilities.CreateEmptyContext());

            Assert.IsTrue(result == true, "Else clause did not run");
        }

        [TestMethod]
        [TestCategory("IfMatch Router")]
        public async Task IfMatch_OnlyTheMainClauseRuns()
        {
            bool result = false;
            await RoutingUtilities.RouteMessage(
                new IfMatch((context) => true, new SimpleRouter(() => result = true), new ErrorRouter()),
                TestUtilities.CreateEmptyContext());

            Assert.IsTrue(result == true, "If clause did not run");
        }

        [TestMethod]
        [TestCategory("IfMatch Router")]
        public async Task Async_RunRoutesSlowFast()
        {

            string state = string.Empty;
            IBotContext bc = TestUtilities.CreateEmptyContext();

            SimpleRouter fast = new SimpleRouter(
                () =>
                {
                    Task.Delay(100);
                    state = "fast";
                });

            SimpleRouter slow = new SimpleRouter(
                () =>
                {
                    Task.Delay(1000);
                    state = "slow";
                });

            FirstRouter f = new FirstRouter();
            f.Add(fast);
            f.Add(slow);

            Route r = await f.GetRoute(bc);
            await r.Action();
            Assert.IsTrue(state == "fast", "State is not fast");

            state = string.Empty;
            FirstRouter f2 = new FirstRouter();
            f2.Add(slow);
            f2.Add(fast);

            Route r2 = await f2.GetRoute(bc);
            await r2.Action();
            Assert.IsTrue(state == "slow", "state is not slow");            
        }
    }
}