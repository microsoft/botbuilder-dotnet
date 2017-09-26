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
                            new Route( () => routed = true)),
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
        public void NullRoutingTests()
        {
            // Should complete and not explode.
            RoutingUtilities.RouteMessage(
                new First(),
                TestUtilities.CreateEmptyContext());

            RoutingUtilities.RouteMessage(
                new First(null),
                TestUtilities.CreateEmptyContext());

            RoutingUtilities.RouteMessage(
                new First(
                    null,
                    RoutingUtilities.NullRouter),
                TestUtilities.CreateEmptyContext());
        }

        [TestMethod]
        [TestCategory("First Router")]
        public void FirstRoutingTests()
        {
            bool routed = false;

            RoutingUtilities.RouteMessage(
                new First(
                    SimpleRouter.Create( () => routed = true )),
                TestUtilities.CreateEmptyContext());

            Assert.IsTrue(routed, "expecting Routed to be TRUE");
        }

        [TestMethod]
        [TestCategory("First Router")]
        public void FirstRoutingTests_RoutePastNull()
        {
            bool routed = false;

            RoutingUtilities.RouteMessage(
                new First(
                    new NullRouter(),
                    null,
                    SimpleRouter.Create(() => routed = true)),
                TestUtilities.CreateEmptyContext());

            Assert.IsTrue(routed, "expecting Routed to be TRUE");
        }
    }
}
