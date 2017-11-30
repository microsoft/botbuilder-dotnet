using Microsoft.Bot.Builder.Conversation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.Bot.Builder.Conversation.RoutingRules;
using static Microsoft.Bot.Builder.Conversation.Routers;
using Microsoft.Bot.Builder.Tests;

namespace Microsoft.Bot.Builder.Conversation.Tests
{
    [TestClass]
    [TestCategory("Routing - First")]
    public class Routing_FirstTests
    {
        [TestMethod]
        public async Task First_NullSet()
        {
            Router r = TryInOrder(null);
            Route route = await r.GetRoute(null);
            Assert.IsNull(route);
        }

        [TestMethod]
        public async Task First_NullRouter()
        {
            Router r = TryInOrder(DoNothing());
            Route route = await r.GetRoute(null);
            Assert.IsNull(route);
        }
        [TestMethod]
        public async Task First_MultipleNullRouters()
        {
            Router r = TryInOrder(
                DoNothing(),
                DoNothing(),
                DoNothing());
            Route route = await r.GetRoute(null);
            Assert.IsNull(route);
        }

        [TestMethod]
        public async Task First_OneHandler()
        {
            bool fired = false;
            Router r = TryInOrder(Simple((context, result) => fired = true));

            Route route = await r.GetRoute(null);
            Assert.IsFalse(fired, "Route should not yet have fired");
            await route.Action(null, null);
            Assert.IsTrue(fired);
        }

        [TestMethod]
        public async Task First_TwoHandlers()
        {
            string whichHandler = "none";
            Router r = TryInOrder(
                Simple((context, result) => whichHandler = "first"),
                Simple((context, result) => whichHandler = "second"));

            Route route = await r.GetRoute(null);
            Assert.IsTrue(whichHandler == "none", "No Route should have fired yet");

            await route.Action(null, null);
            Assert.IsTrue(whichHandler == "first", "Incorrect Handler Called");
        }

        [TestMethod]
        public async Task First_TwoHandlersFirstIsNull()
        {
            string whichHandler = "none";
            Router r = TryInOrder(
                DoNothing(),
                Simple((context, result) => whichHandler = "second"));

            Route route = await r.GetRoute(null);
            Assert.IsTrue(whichHandler == "none", "No Route should have fired yet");

            await route.Action(null, null);
            Assert.IsTrue(whichHandler == "second", "Incorrect Handler Called");
        }

        [TestMethod]
        public async Task First_OneRouter()
        {
            bool routerFired = false;
            bool routeFired = false;

            Router first = new Router(async (context, routePaths) =>
            {
                routerFired = true;
                return new Route(async (ctx, result) => { routeFired = true; });
            });

            Router r = TryInOrder(first);

            Route route = await r.GetRoute(null);
            Assert.IsTrue(routerFired, "Route did not evaluate");
            Assert.IsNotNull(route, "no route returned");

            await route.Action(null, null);

            Assert.IsTrue(routeFired, "Route did not fire");
        }

        [TestMethod]
        public async Task First_TwoRouters()
        {
            bool routerFired = false;
            bool routeFired = false;

            Router second = new Router(async (context, routePaths) =>
            {
                routerFired = true;
                return new Route(async (cxt, match) => { routeFired = true; });
            });

            Router r = TryInOrder(
                DoNothing(),
                second);

            Route route = await r.GetRoute(null);
            Assert.IsTrue(routerFired, "Route did not evaluate");
            Assert.IsNotNull(route, "no route returned");

            await route.Action(null, null);

            Assert.IsTrue(routeFired, "Route did not fire");
        }

        [TestMethod]
        public async Task First_Async_RunRoutesSlowFast()
        {
            string state = string.Empty;
            IBotContext bc = TestUtilities.CreateEmptyContext();

            Router fast = Simple((context, result) => { Task.Delay(100); state = "fast"; });
            Router slow = Simple((context, result) => { Task.Delay(1000); state = "slow"; });

            Router first = TryInOrder(fast, slow);
            Route route = await first.GetRoute(bc);
            await route.Action(null, null);
            Assert.IsTrue(state == "fast", "State is not fast");

            state = string.Empty;
            Router second = TryInOrder(slow, fast);
            Route route2 = await second.GetRoute(bc);
            await route2.Action(null, null);
            Assert.IsTrue(state == "slow", "state is not slow");
        }
    }
}
