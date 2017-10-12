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
    [TestCategory("Routing - Basic")]
    public class Routing_BasicTests
    {
        [TestMethod]
        public async Task Routing_NullRouterTests()
        {
            NullRouter nullRouter = new NullRouter();
            BotContext bc = TestUtilities.CreateEmptyContext();
            Route r = await nullRouter.GetRoute(bc);
            Assert.IsNull(r, "Null Route Expected. Found a route.");
        }

        [TestMethod]
        public async Task Routing_SimpleRouteTest()
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
        public async Task Routing_NullMessageRouter()
        {
            //Should not explode. :)
            await RoutingUtilities.RouteMessage(
                RoutingUtilities.NullRouter,
                TestUtilities.CreateEmptyContext());
        }

        [TestMethod]
        public async Task Routing_RouteToTestRouter()
        {
            SimpleTestRouter r = new SimpleTestRouter();

            Assert.IsFalse(r.Routed, "Expecting Routed to be FALSE");
            await RoutingUtilities.RouteMessage(
                r,
                TestUtilities.CreateEmptyContext());
            Assert.IsTrue(r.Routed, "Expecting Routed to be TRUE");
        }

        [TestMethod]
        public async Task Routing_SimpleRouterRoute()
        {
            bool routed = false;

            await RoutingUtilities.RouteMessage(
                new SimpleRouter(
                    () => routed = true),
                TestUtilities.CreateEmptyContext());

            Assert.IsTrue(routed, "Expecting Routed to be TRUE");
        }

        [TestMethod]
        public async Task Routing_SimpleRouterRouteWithContext()
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
        public async Task Routing_AnonymousRouterTest()
        {
            bool routed = false;

            await RoutingUtilities.RouteMessage(
                new AnonymousRouter(async (context) =>
                            new Route(async () => routed = true)),
                TestUtilities.CreateEmptyContext());

            Assert.IsTrue(routed, "expecting Routed to be TRUE");
        }

        [TestMethod]
        public async Task Routing_HandlerToRouterConversionTest()
        {
            bool routed = false;

            IHandler handler = SimpleHandler.Create(() => routed = true);

            await RoutingUtilities.RouteMessage(
                handler.AsRouter(),
                TestUtilities.CreateEmptyContext());

            Assert.IsTrue(routed, "expecting Routed to be TRUE");
        }
    }
}