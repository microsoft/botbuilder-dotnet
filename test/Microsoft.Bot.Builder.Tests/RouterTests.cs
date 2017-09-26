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
        public void NullRouterTests()
        {
            NullRouter nullRouter = new NullRouter();
            BotContext bc = TestUtilities.CreateEmptyContext();
            Route r = nullRouter.GetRoute(bc);
            Assert.IsNull(r, "Null Route Expected. Found a route.");
        }

        [TestMethod]
        public void SimpleRouteTest()
        {
            SimpleTestRouter tr = new SimpleTestRouter();
            Route r = tr.GetRoute(TestUtilities.CreateEmptyContext());
            Assert.IsNotNull(r, "Expected a Route. Did not get one.");

            Assert.IsFalse(tr.Routed, "Expected Routed to initially be false");
            r.Action();
            Assert.IsTrue(tr.Routed, "Expected Routed to initially be true");
        }

        class SimpleTestRouter : IRouter
        {
            public bool Routed { get; set; } = false;

            public Route GetRoute(IBotContext context)
            {
                return new Route(() => Routed = true);
            }
        }

        [TestMethod]
        public void NullMessageRouter()
        {
            //Should not explode. :)
            RoutingUtilities.RouteMessage(
                RoutingUtilities.NullRouter,
                TestUtilities.CreateEmptyContext());
        }

        [TestMethod]
        public void RouteToTestRouter()
        {
            SimpleTestRouter r = new SimpleTestRouter();

            Assert.IsFalse(r.Routed, "Expecting Routed to be FALSE");
            RoutingUtilities.RouteMessage(
                r,
                TestUtilities.CreateEmptyContext());
            Assert.IsTrue(r.Routed, "Expecting Routed to be TRUE");
        }
        [TestMethod]
        public void SimpleRouterRoute()
        {
            bool routed = false;

            RoutingUtilities.RouteMessage(
                new SimpleRouter(
                    () => routed = true),
                TestUtilities.CreateEmptyContext());

            Assert.IsTrue(routed, "Expecting Routed to be TRUE");
        }

        public void SimpleRouterRouteWithContext()
        {
            bool routed = false;
            IBotContext bc = TestUtilities.CreateEmptyContext();
            bc.State["test"] = "foo";

            RoutingUtilities.RouteMessage(
                new SimpleRouter(
                    (context) => routed = (bc.State["test"] == true)),
                TestUtilities.CreateEmptyContext());

            Assert.IsTrue(routed, "Expecting Routed to be TRUE");
        }

        [TestMethod]
        public void RouterToRouterConversionTest()
        {
            bool routed = false;

            RoutingUtilities.RouteMessage(
                RoutingUtilities.ToRouter(
                        new SimpleRouter(
                            () => routed = true)),
                TestUtilities.CreateEmptyContext());

            Assert.IsTrue(routed, "expecting Routed to be TRUE");
        }

        [TestMethod]
        public void AnonymousRouterTest()
        {
            bool routed = false;

            RoutingUtilities.RouteMessage(
                new AnonymousRouter(
                       (context) =>
                            new Route(() => routed = true)),
                TestUtilities.CreateEmptyContext());

            Assert.IsTrue(routed, "expecting Routed to be TRUE");

        }
        [TestMethod]
        public void HandlerToRouterConversionTest()
        {
            bool routed = false;

            IHandler handler = SimpleHandler.Create(() => routed = true);

            RoutingUtilities.RouteMessage(
                RoutingUtilities.ToRouter(handler),
                TestUtilities.CreateEmptyContext());

            Assert.IsTrue(routed, "expecting Routed to be TRUE");
        }
        [TestMethod]
        [TestCategory("First Router")]
        public void First_NullRoutingTests()
        {
            // Should complete and not explode.
            RoutingUtilities.RouteMessage(
                new FirstRouter(),
                TestUtilities.CreateEmptyContext());

            RoutingUtilities.RouteMessage(
                new FirstRouter(
                    RoutingUtilities.NullRouter),
                TestUtilities.CreateEmptyContext());

            RoutingUtilities.RouteMessage(
                new FirstRouter()
                    .Add(RoutingUtilities.NullRouter),
                TestUtilities.CreateEmptyContext());

            RoutingUtilities.RouteMessage(
                new FirstRouter()
                    .Add(RoutingUtilities.NullRouter)
                    .Add(RoutingUtilities.NullRouter)
                    .Add(RoutingUtilities.NullRouter),
                TestUtilities.CreateEmptyContext());

        }

        [TestMethod]
        [TestCategory("First Router")]
        public void First_RoutingTests()
        {
            bool routed = false;

            RoutingUtilities.RouteMessage(
                new FirstRouter(
                    SimpleRouter.Create(() => routed = true)),
                TestUtilities.CreateEmptyContext());

            Assert.IsTrue(routed, "expecting Routed to be TRUE");

            routed = false;
            RoutingUtilities.RouteMessage(
                new FirstRouter()
                    .Add(SimpleRouter.Create(() => routed = true)),
                TestUtilities.CreateEmptyContext());
            Assert.IsTrue(routed, "expecting Routed to be TRUE");
        }

        [TestMethod]
        [TestCategory("First Router")]
        public void First_RoutingTests_RoutePastNull()
        {
            bool routed = false;

            RoutingUtilities.RouteMessage(
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
        public void First_DifferentRouterTypes()
        {
            bool routed = false;

            IRouter simpleViaStatic = SimpleRouter.Create(() => routed = true);
            IRouter simpleViaCtor = new SimpleRouter(() => routed = true);
            IRouter nullRouterviaStatic = RoutingUtilities.NullRouter;
            IRouter nullRouterviaCtor = new NullRouter();
            IRouter anonymousRouter = new AnonymousRouter(
                       (context) =>
                            new Route(
                                () => routed = true));
            IRouter testRouter = new SimpleTestRouter();
        }

        [TestMethod]
        [TestCategory("Best Router")]
        public void Best_NullRoutingTests()
        {
            // Should complete and not explode.
            RoutingUtilities.RouteMessage(
                new BestRouter(),
                TestUtilities.CreateEmptyContext());

            RoutingUtilities.RouteMessage(
                new BestRouter()
                    .Add((IHandler)null)
                    .Add((IRouter)null)
                    .Add(RoutingUtilities.NullRouter),
                TestUtilities.CreateEmptyContext());
        }

        [TestMethod]
        [TestCategory("Best Router")]
        public void Best_RouteToHandler()
        {
            bool routed = false;
            // Should complete and not explode.
            RoutingUtilities.RouteMessage(
                new BestRouter()
                    .Add(new SimpleHandler(() => routed = true)),
                TestUtilities.CreateEmptyContext());

            Assert.IsTrue(routed, "Expected routed to be TRUE");
        }


        [TestMethod]
        [TestCategory("Best Router")]
        public void Best_RoutePastNullToHandler()
        {
            bool routed = false;
            // Should complete and not explode.
            RoutingUtilities.RouteMessage(
                new BestRouter()
                    .Add(new NullRouter())
                    .Add((IRouter)null)
                    .Add((IHandler)null)
                    .Add(new SimpleHandler(() => routed = true)),
                TestUtilities.CreateEmptyContext());

            Assert.IsTrue(routed, "Expected routed to be TRUE");
        }

        [TestMethod]
        [TestCategory("Best Router")]
        public void Best_StopAtFirstMaxScoreRouter()
        {
            bool routed = false;

            // Should complete and not explode. Note, if the BestRouter passes the SimpleHandler, the Error Router
            // will throw and cause the test to fail. 
            RoutingUtilities.RouteMessage(
                new BestRouter()
                    .Add(new NullRouter())
                    .Add(new SimpleHandler(() => routed = true))
                    .Add(new ErrorRouter()),
                TestUtilities.CreateEmptyContext());

            Assert.IsTrue(routed, "Expected routed to be TRUE");
        }

        [TestMethod]
        [TestCategory("Best Router")]
        [ExpectedException(typeof(InvalidOperationException), "No Exception found. Test failed.")]
        public void Best_FailOnExceptionDuringRouting()
        {
            // Should complete and not explode. Note, if the BestRouter passes the SimpleHandler, the Error Router
            // will throw and cause the test to fail. 
            RoutingUtilities.RouteMessage(
                new BestRouter()
                    .Add(new ErrorRouter()),
                TestUtilities.CreateEmptyContext());
        }

        [TestMethod]
        [TestCategory("Best Router")]
        public void Best_HigherScoreFirst()
        {
            string whichRan = string.Empty;
            RoutingUtilities.RouteMessage(
                new BestRouter()
                    .Add(new ScoredRouter(() => whichRan = "0.5", 0.5))
                    .Add(new ScoredRouter(() => whichRan = "0.4", 0.4)),
                TestUtilities.CreateEmptyContext());

            Assert.IsTrue(whichRan == "0.5", "Incorrect score was run");
        }

        [TestMethod]
        [TestCategory("Best Router")]
        public void Best_HigherScoreLast()
        {
            string whichRan = string.Empty;
            RoutingUtilities.RouteMessage(
                new BestRouter()
                    .Add(new ScoredRouter(() => whichRan = "0.4", 0.4))
                    .Add(new ScoredRouter(() => whichRan = "0.5", 0.5)),
                TestUtilities.CreateEmptyContext());

            Assert.IsTrue(whichRan == "0.5", "Incorrect score was run");
        }

        [TestMethod]
        [TestCategory("Best Router")]
        public void Best_HigherScoreMiddle()
        {
            string whichRan = string.Empty;
            RoutingUtilities.RouteMessage(
                new BestRouter()
                    .Add(new ScoredRouter(() => whichRan = "0.4", 0.4))
                    .Add(new ScoredRouter(() => whichRan = "0.9", 0.9))
                    .Add(new ScoredRouter(() => whichRan = "0.5", 0.5)),
                TestUtilities.CreateEmptyContext());

            Assert.IsTrue(whichRan == "0.9", "Incorrect score was run");
        }

        [TestMethod]
        [TestCategory("Best Router")]
        public void Best_TiedScores()
        {
            // Defined behavior is that if scores are tied, the first one wins. 
            string whichRan = string.Empty;
            RoutingUtilities.RouteMessage(
                new BestRouter()
                    .Add(new ScoredRouter(() => whichRan = "first", 0.5))
                    .Add(new ScoredRouter(() => whichRan = "second", 0.5)),
                TestUtilities.CreateEmptyContext());

            Assert.IsTrue(whichRan == "first", "Incorrect score was run");
        }

        [TestMethod]
        [TestCategory("IfMatch Router")]
        [ExpectedException(typeof(ArgumentNullException), "Expected ArgumentNullException")]
        public void IfMatch_RequireParemters()
        {
            RoutingUtilities.RouteMessage(
                new IfMatch(null, null),
                TestUtilities.CreateEmptyContext());
        }

        [TestMethod]
        [TestCategory("IfMatch Router")]
        public void IfMatch_ExecuteOnMatch()
        {
            bool result = false;
            RoutingUtilities.RouteMessage(
                new IfMatch(
                    (context) => true,
                    new SimpleRouter(() => result = true)),
                TestUtilities.CreateEmptyContext());

            Assert.IsTrue(result == true, "Expected result to be TRUE");
        }

        [TestMethod]
        [TestCategory("IfMatch Router")]
        public void IfMatch_DoNotExecuteIfNoMatch()
        {
            bool result = false;
            RoutingUtilities.RouteMessage(
                new IfMatch(
                    (context) => false,
                    new SimpleRouter(() => result = true)),
                TestUtilities.CreateEmptyContext());

            Assert.IsTrue(result == false, "Expected result to be FALSE. The Predicate incorrectly matched.");
        }

        [TestMethod]
        [TestCategory("IfMatch Router")]
        public void IfMatch_NoMatchNoElseClause()
        {
            // should complete and never emit on false predicate when 'else' router doesn't exist            
            RoutingUtilities.RouteMessage(
                new IfMatch((context) => false, new ErrorRouter()),
                TestUtilities.CreateEmptyContext());

            // No Exception means the test has passed. 
        }

        [TestMethod]
        [TestCategory("IfMatch Router")]
        public void IfMatch_NoMatchRunsElseClauseNullRouter()
        {
            // should complete and never emit on false predicate when 'else' router doesn't route            
            RoutingUtilities.RouteMessage(
                new IfMatch((context) => false, new ErrorRouter(), new NullRouter()),
                TestUtilities.CreateEmptyContext());

            // No Exception means the test has passed. 
        }

        [TestMethod]
        [TestCategory("IfMatch Router")]
        public void IfMatch_NoMatchRunsElseClause()
        {
            bool result = false;
            RoutingUtilities.RouteMessage(
                new IfMatch((context) => false, new ErrorRouter(), new SimpleRouter(() => result = true)),
                TestUtilities.CreateEmptyContext());

            Assert.IsTrue(result == true, "Else clause did not run");
        }

        [TestMethod]
        [TestCategory("IfMatch Router")]
        public void IfMatch_OnlyTheMainClauseRuns()
        {
            bool result = false;
            RoutingUtilities.RouteMessage(
                new IfMatch((context) => true, new SimpleRouter(() => result = true), new ErrorRouter()),
                TestUtilities.CreateEmptyContext());

            Assert.IsTrue(result == true, "If clause did not run");
        }

    }
}