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
    [TestCategory("Routing - First")]
    public class Routing_FirstTests
    {
        [TestMethod]
        public async Task First_NullSet()
        {
            IRouter r = First(null);
            Route route = await r.GetRoute(null);
            Assert.IsNull(route);
        }

        [TestMethod]
        public async Task First_NullRouter()
        {
            IRouter r = First(Router.NoRouter());
            Route route = await r.GetRoute(null);
            Assert.IsNull(route);
        }
        [TestMethod]
        public async Task First_MultipleNullRouters()
        {
            IRouter r = First(
                Router.NoRouter(),
                Router.NoRouter(),
                Router.NoRouter());
            Route route = await r.GetRoute(null);
            Assert.IsNull(route);
        }

        [TestMethod]
        public async Task First_OneHandler()
        {
            bool fired = false;
            IRouter r = First(Simple(() => fired = true));

            Route route = await r.GetRoute(null);
            Assert.IsFalse(fired, "Route should not yet have fired");
            await route.Action();
            Assert.IsTrue(fired);
        }

        [TestMethod]
        public async Task First_TwoHandlers()
        {
            string whichHandler = "none";
            IRouter r = First(
                Simple(() => whichHandler = "first"),
                Simple(() => whichHandler = "second"));

            Route route = await r.GetRoute(null);
            Assert.IsTrue(whichHandler == "none", "No Route should have fired yet");

            await route.Action();
            Assert.IsTrue(whichHandler == "first", "Incorrect Handler Called");
        }

        [TestMethod]
        public async Task First_TwoHandlersFirstIsNull()
        {
            string whichHandler = "none";
            IRouter r = First(
                Router.NoRouter(),
                Simple(() => whichHandler = "second"));

            Route route = await r.GetRoute(null);
            Assert.IsTrue(whichHandler == "none", "No Route should have fired yet");

            await route.Action();
            Assert.IsTrue(whichHandler == "second", "Incorrect Handler Called");
        }

        [TestMethod]
        public async Task First_OneRouter()
        {
            bool routerFired = false;
            bool routeFired = false;

            IRouter first = new Router(async (context) =>
            {
                routerFired = true;
                return new Route(async () => { routeFired = true; });
            });

            IRouter r = First(first);

            Route route = await r.GetRoute(null);
            Assert.IsTrue(routerFired, "Route did not evaluate");
            Assert.IsNotNull(route, "no route returned");

            await route.Action();

            Assert.IsTrue(routeFired, "Route did not fire");
        }

        [TestMethod]
        public async Task First_TwoRouters()
        {
            bool routerFired = false;
            bool routeFired = false;

            IRouter second = new Router(async (context) =>
            {
                routerFired = true;
                return new Route(async () => { routeFired = true; });
            });

            IRouter r = First(
                Router.NoRouter(),
                second);

            Route route = await r.GetRoute(null);
            Assert.IsTrue(routerFired, "Route did not evaluate");
            Assert.IsNotNull(route, "no route returned");

            await route.Action();

            Assert.IsTrue(routeFired, "Route did not fire");
        }

        [TestMethod]
        public async Task First_Async_RunRoutesSlowFast()
        {
            string state = string.Empty;
            IBotContext bc = TestUtilities.CreateEmptyContext();

            IRouterOrHandler fast = Simple(() => { Task.Delay(100); state = "fast"; });
            IRouterOrHandler slow = Simple(() => { Task.Delay(1000); state = "slow"; });

            IRouter first = First(fast, slow);
            Route r = await first.GetRoute(bc);
            await r.Action();
            Assert.IsTrue(state == "fast", "State is not fast");

            state = string.Empty;
            IRouter second = First(slow, fast);
            Route r2 = await second.GetRoute(bc);
            await r2.Action();
            Assert.IsTrue(state == "slow", "state is not slow");
        }      
    }
}
