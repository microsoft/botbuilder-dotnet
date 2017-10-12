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
    [TestCategory("Routing - First")]
    public class Routing_FirstTests
    {
        [TestMethod]
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
    }
}
