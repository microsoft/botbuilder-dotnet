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
    [TestCategory("Routing - Best")]
    public class Routing_BestTests
    {
        [TestMethod]
        
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
    }
}
