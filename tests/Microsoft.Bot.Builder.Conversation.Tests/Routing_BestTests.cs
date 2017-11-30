using Microsoft.Bot.Builder.Conversation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.Bot.Builder.Conversation.RoutingRules;
using static Microsoft.Bot.Builder.Conversation.Routers;

namespace Microsoft.Bot.Builder.Conversation.Tests
{
    [TestClass]
    [TestCategory("Routing - Best")]
    public class Routing_BestTests
    {
        [TestMethod]
        public async Task Best_NullRoutingTests()
        {
            Router r = TryBest(
                    DoNothing(),
                    null,
                    (Router)null);

            Route route = await r.GetRoute(null);
            Assert.IsNull(route);
        }

        [TestMethod]
        public async Task Best_RouteToHandler()
        {
            bool routed = false;
            Router r = TryBest(Simple((context, match) => routed = true));

            Route route = await r.GetRoute(null);
            Assert.IsFalse(routed, "should not have have routed yet");
            await route.Action(null, null);
            Assert.IsTrue(routed, "Expected routed to be TRUE");
        }


        [TestMethod]
        public async Task Best_RoutePastNullToHandler()
        {
            bool routed = false;
            Router r = TryBest(
                    DoNothing(),
                    (Router)null,
                    Simple((context, result) => routed = true));

            Route route = await r.GetRoute(null);
            Assert.IsFalse(routed, "should have have routed yet");
            await route.Action(null, null);
            Assert.IsTrue(routed, "Expected routed to be TRUE");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "No Exception found. Test failed.")]
        public async Task Best_FailOnExceptionDuringRouting()
        {
            Router r = TryBest(Error());
            await r.GetRoute(null);
            Assert.Fail("expected the error router to throw on evaulation");
        }

        [TestMethod]
        public async Task Best_HigherScoreFirst()
        {
            string whichRan = string.Empty;
            Router r = TryBest(
                Scored((context, result) => whichRan = "0.5", 0.5),
                Scored((context, result) => whichRan = "0.4", 0.4));

            Route route = await r.GetRoute(null);
            await route.Action(null, null);

            Assert.IsTrue(whichRan == "0.5", "Incorrect score was run");
        }

        [TestMethod]
        public async Task Best_HigherScoreLast()
        {
            string whichRan = string.Empty;
            Router r = TryBest(
                Scored((context, result) => whichRan = "0.4", 0.4),
                Scored((context, result) => whichRan = "0.5", 0.5));

            Route route = await r.GetRoute(null);
            await route.Action(null, null);
            Assert.IsTrue(whichRan == "0.5", "Incorrect score was run");
        }

        [TestMethod]
        public async Task Best_HigherScoreMiddle()
        {
            string whichRan = string.Empty;
            Router r = TryBest(
                Scored((context, result) => whichRan = "0.4", 0.4),
                Scored((context, result) => whichRan = "0.9", 0.9),
                Scored((context, result) => whichRan = "0.5", 0.5));

            Route route = await r.GetRoute(null);
            await route.Action(null, null);
            Assert.IsTrue(whichRan == "0.9", "Incorrect score was run");
        }

        [TestMethod]
        public async Task Best_TiedScores()
        {
            string whichRan = string.Empty;
            Router r = TryBest(
                Scored((context, result) => whichRan = "first", 0.5),
                Scored((context, result) => whichRan = "second", 0.5));

            Route route = await r.GetRoute(null);
            await route.Action(null, null);

            Assert.IsTrue(whichRan == "first", "Incorrect score was run");
        }
    }
}
