using Microsoft.Bot.Builder.Prague;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.Bot.Builder.Prague.RoutingRules;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("Routing - Best")]
    public class Routing_BestTests
    {
        [TestMethod]
        public async Task Best_NullRoutingTests()
        {
            IRouter r = Best(
                    Router.NoRouter(),
                    null,
                    (IHandler)null,
                    (IRouter)null);

            Route route = await r.GetRoute(null);            
            Assert.IsNull(route);
        }

        [TestMethod]
        public async Task Best_RouteToHandler()
        {
            bool routed = false;
            IRouter r = Best(new SimpleHandler(() => routed = true));

            Route route = await r.GetRoute(null);
            Assert.IsFalse(routed, "should have have routed yet");
            await route.Action();
            Assert.IsTrue(routed, "Expected routed to be TRUE");
        }


        [TestMethod]
        public async Task Best_RoutePastNullToHandler()
        {
            bool routed = false;
            IRouter r = Best(
                    Router.NoRouter(),
                    (IRouter)null,
                    (IHandler)null,
                    new SimpleHandler(() => routed = true));

            Route route = await r.GetRoute(null);
            Assert.IsFalse(routed, "should have have routed yet");
            await route.Action();
            Assert.IsTrue(routed, "Expected routed to be TRUE");
        }
      
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "No Exception found. Test failed.")]
        public async Task Best_FailOnExceptionDuringRouting()
        {
            IRouter r = Best(Error());
            await r.GetRoute(null);
            Assert.Fail("expected the error router to throw on evaulation");
        }

        [TestMethod]
        public async Task Best_HigherScoreFirst()
        {
            string whichRan = string.Empty;
            IRouter r = Best(
                new ScoredRouter(async () => whichRan = "0.5", 0.5),
                new ScoredRouter(async () => whichRan = "0.4", 0.4)
                );

            Route route = await r.GetRoute(null);
            await route.Action();

            Assert.IsTrue(whichRan == "0.5", "Incorrect score was run");
        }

        [TestMethod]
        public async Task Best_HigherScoreLast()
        {
            string whichRan = string.Empty;
            IRouter r = Best(
                new ScoredRouter(async () => whichRan = "0.4", 0.4),
                new ScoredRouter(async () => whichRan = "0.5", 0.5)                
                );

            Route route = await r.GetRoute(null);
            await route.Action();
            Assert.IsTrue(whichRan == "0.5", "Incorrect score was run");
        }

        [TestMethod]
        public async Task Best_HigherScoreMiddle()
        {
            string whichRan = string.Empty;
            IRouter r = Best(
                new ScoredRouter(async () => whichRan = "0.4", 0.4),
                new ScoredRouter(async () => whichRan = "0.9", 0.9),
                new ScoredRouter(async () => whichRan = "0.5", 0.5)
                );

            Route route = await r.GetRoute(null);
            await route.Action();
            Assert.IsTrue(whichRan == "0.9", "Incorrect score was run");
        }

        [TestMethod]
        public async Task Best_TiedScores()
        {
            string whichRan = string.Empty;
            IRouter r = Best(
                new ScoredRouter(async () => whichRan = "first", 0.5),
                new ScoredRouter(async () => whichRan = "second", 0.5)
                );

            Route route = await r.GetRoute(null);
            await route.Action();

            Assert.IsTrue(whichRan == "first", "Incorrect score was run");
        }
    }
}
