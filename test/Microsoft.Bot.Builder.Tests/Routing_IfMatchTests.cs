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
    [TestCategory("Routing - IfMatch")]
    public class Routing_IfMatchTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "Expected ArgumentNullException")]
        public async Task IfMatch_RequireParemters()
        {
            await RoutingUtilities.RouteMessage(
                new IfMatch((IfMatch.Condition)null, null),
                TestUtilities.CreateEmptyContext());
        }

        [TestMethod]
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
        public async Task IfMatch_NoMatchNoElseClause()
        {
            // should complete and never emit on false predicate when 'else' router doesn't exist            
            await RoutingUtilities.RouteMessage(
                new IfMatch((context) => false, new ErrorRouter()),
                TestUtilities.CreateEmptyContext());

            // No Exception means the test has passed. 
        }

        [TestMethod]
        public async Task IfMatch_NoMatchRunsElseClauseNullRouter()
        {
            // should complete and never emit on false predicate when 'else' router doesn't route            
            await RoutingUtilities.RouteMessage(
                new IfMatch((context) => false, new ErrorRouter(), new NullRouter()),
                TestUtilities.CreateEmptyContext());

            // No Exception means the test has passed. 
        }

        [TestMethod]
        public async Task IfMatch_NoMatchRunsElseClause()
        {
            bool result = false;
            await RoutingUtilities.RouteMessage(
                new IfMatch((context) => false, new ErrorRouter(), new SimpleRouter(() => result = true)),
                TestUtilities.CreateEmptyContext());

            Assert.IsTrue(result == true, "Else clause did not run");
        }

        [TestMethod]
        public async Task IfMatch_OnlyTheMainClauseRuns()
        {
            bool result = false;
            await RoutingUtilities.RouteMessage(
                new IfMatch((context) => true, new SimpleRouter(() => result = true), new ErrorRouter()),
                TestUtilities.CreateEmptyContext());

            Assert.IsTrue(result == true, "If clause did not run");
        }

        [TestMethod]
        public async Task IfMatch_Async_RunRoutesSlowFast()
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
