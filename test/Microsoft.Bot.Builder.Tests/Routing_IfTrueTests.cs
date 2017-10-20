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
    [TestCategory("Routing - IfTrue")]
    public class Routing_IfTrueTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "Expected ArgumentNullException")]
        public async Task IfTrue_RequireParameters()
        {
            IfTrue((Condition)null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "Expected ArgumentNullException")]
        public async Task IfTrue_RequireParametersAsync()
        {
            IfTrue((ConditionAsync)null, null);
        }

        [TestMethod]
        public async Task IfTrue_ExecuteOnMatch()
        {
            bool result = false;
            IRouter r = IfTrue((context) => true, new SimpleRouter(() => result = true));
            Route route = await r.GetRoute(null);
            await route.Action();
            Assert.IsTrue(result == true, "Expected result to be TRUE");
        }

        [TestMethod]
        public async Task IfTrue_NoMatchNoElseClause()
        {
            IRouter r = IfTrue((context) => false, Error());
            Route route = await r.GetRoute(null);
            Assert.IsNull(route, "Should be no route");
        }

        [TestMethod]
        public async Task IfTrue_NoMatchRunsElseClauseNullRouter()
        {
            IRouter r = IfTrue((context) => false, Error(), Router.NoRouter());
            Route route = await r.GetRoute(null);
            Assert.IsNull(route, "Should be no route");
        }

        [TestMethod]
        public async Task IfTrue_NoMatchRunsElseClause()
        {
            bool result = false;
            IRouter r = IfTrue((context) => false, Error(), new SimpleRouter(() => result = true));

            Route route = await r.GetRoute(null);
            await route.Action();

            Assert.IsTrue(result == true, "Else clause did not run");
        }

        [TestMethod]
        public async Task IfTrue_OnlyTheMainClauseRuns()
        {
            bool result = false;
            IRouter r = IfTrue((context) => true, new SimpleRouter(() => result = true), Error());
            Route route = await r.GetRoute(null);
            await route.Action();

            Assert.IsTrue(result == true, "If clause did not run");
        }
    }
}
