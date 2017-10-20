using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Bot.Builder.Prague;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using static Microsoft.Bot.Builder.Prague.Routers;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("Router")]
    public class Routing_Router
    {
        [TestMethod]
        [TestCategory("Router - Basic")]
        public async Task Router_GetRoute()
        {
            bool routerFired = false;
            bool routeFired = false;

            Router router = new Router(
               async (context) =>
               {
                   routerFired = true;
                   return new Route(async () =>
                   {
                       routeFired = true;
                   });
               });

            Route r = await router.GetRoute(null);

            Assert.IsTrue(routerFired, "Router did not evaluate the Route");
            Assert.IsFalse(routeFired, "Router has incorrectly fired the Route");

            await r.Action();

            Assert.IsTrue(routeFired, "Route Failed to fire");
        }

        [TestMethod]
        [TestCategory("Router - Basic")]
        public async Task Router_NullRoute()
        {
            bool routerFired = false;

            Router router = new Router(
               async (context) =>
               {
                   routerFired = true;
                   return null;
               });

            Route route = await router.GetRoute(null);

            Assert.IsTrue(routerFired, "Router did not evaluate the Route");
            Assert.IsNull(route, "Route should be null");
        }


        [TestMethod]
        [TestCategory("Router - DoBefore")]
        public async Task Router_DoBefore_TwoHandlers()
        {
            IList<string> orderMatters = new List<string>();

            Handler one = Simple(() => orderMatters.Add("one"));
            Handler two = Simple(() => orderMatters.Add("two"));

            Router foo = Router.DoBefore(one, two);
            Route route = await foo.GetRoute(null);
            await route.Action();

            Assert.IsTrue(orderMatters.Count == 2);
            Assert.IsTrue(orderMatters[0] == "one");
            Assert.IsTrue(orderMatters[1] == "two");
        }

        [TestMethod]
        [TestCategory("Router - DoBefore")]
        public async Task Router_DoBefore_NullRoute()
        {
            IList<string> orderMatters = new List<string>();
            bool routerFired = false;

            Handler one = Simple(() => orderMatters.Add("one"));
            Router nullRouter = new Router(
               async (context) =>
               {
                   routerFired = true;

                   // returning a null route here means DoBefore rule should not run
                   // If an actual Route was returned, then the Before would be triggered
                   return null;
               }
           );

            Router foo = Router.DoBefore(one, nullRouter);
            Route route = await foo.GetRoute(null);
            Assert.IsNull(route, "Incorrectly got a route back.");
            Assert.IsTrue(orderMatters.Count == 0);
            Assert.IsTrue(routerFired, "Router did not fire");
        }

        [TestMethod]
        [TestCategory("Router - DoBefore")]
        public async Task Router_DoBefore_OneRouter()
        {
            IList<string> orderMatters = new List<string>();
            Handler one = Simple(() => orderMatters.Add("handler"));

            bool routerFired = false;
            bool routeFired = false;

            Router router = new Router(
                async (context) =>
                {
                    routerFired = true;
                    return new Route(async () =>
                    {
                        routeFired = true;
                        orderMatters.Add("router");
                    });
                }
            );

            Router foo = Router.DoBefore(one, router);
            Route route = await foo.GetRoute(null);

            // At this point, the original router ran and returned a route. 
            Assert.IsTrue(routerFired, "Router did not fire");
            Assert.IsFalse(routeFired, "Route has already fired. Shouldn't happen yet.");

            // Now it's time to actually run the route. 
            await route.Action();
            Assert.IsTrue(orderMatters.Count == 2);
            Assert.IsTrue(orderMatters[0] == "handler");
            Assert.IsTrue(orderMatters[1] == "router");

            Assert.IsTrue(routeFired, "Route did not fire");
        }

        [TestMethod]
        [TestCategory("Router - DoAfter")]
        public async Task Router_DoAfter_TwoHandlers()
        {
            IList<string> orderMatters = new List<string>();

            var one = Simple(() => orderMatters.Add("one"));
            var two = Simple(() => orderMatters.Add("two"));

            Router foo = Router.DoAfter(one, two);
            Route route = await foo.GetRoute(null);
            await route.Action();

            Assert.IsTrue(orderMatters.Count == 2);
            Assert.IsTrue(orderMatters[0] == "one");
            Assert.IsTrue(orderMatters[1] == "two");
        }

        [TestMethod]
        [TestCategory("Router - DoAfter")]
        public async Task Router_DoAfter_TwoHandlers_PathUpdates()
        {
            IList<string> orderMatters = new List<string>();
            string[] routingPath = new List<string>().ToArray();

            Handler one = Simple(() => orderMatters.Add("one"));
            Handler two = Simple(() => orderMatters.Add("two"));

            Router foo = Router.DoAfter(one, two);
            Route route = await foo.GetRoute(null, routingPath);
            await route.Action();

            Assert.IsTrue(orderMatters.Count == 2);
            Assert.IsTrue(orderMatters[0] == "one");
            Assert.IsTrue(orderMatters[1] == "two");
        }

        [TestMethod]
        [TestCategory("Router - DoAfter")]
        public async Task Router_DoAfter_NullRoute()
        {
            IList<string> orderMatters = new List<string>();
            bool routerFired = false;

            var one = Simple(() => orderMatters.Add("one"));
            Router nullRouter = new Router(
               async (context) =>
               {
                   routerFired = true;

                   // returning a null route here means DoBefore rule should not run
                   // If an actual Route was returned, then the Before would be triggered
                   return null;
               }
           );

            Router foo = Router.DoAfter(nullRouter, one);
            Route route = await foo.GetRoute(null);
            Assert.IsNull(route, "Incorrectly got a route back.");
            Assert.IsTrue(orderMatters.Count == 0);
            Assert.IsTrue(routerFired, "Router did not fire");
        }

        [TestMethod]
        [TestCategory("Router - DoAfter")]
        public async Task Router_DoAfter_OneRouter()
        {
            IList<string> orderMatters = new List<string>();
            var one = Simple(() => orderMatters.Add("handler"));

            bool routerFired = false;
            bool routeFired = false;

            Router router = new Router(
                async (context) =>
                {
                    routerFired = true;
                    return new Route(async () =>
                    {
                        routeFired = true;
                        orderMatters.Add("router");
                    });
                }
            );

            Router foo = Router.DoAfter(router, one);
            Route route = await foo.GetRoute(null);

            // At this point, the original router ran and returned a route. 
            Assert.IsTrue(routerFired, "Router did not fire");
            Assert.IsFalse(routeFired, "Route has already fired. Shouldn't happen yet.");

            // Now it's time to actually run the route. 
            await route.Action();
            Assert.IsTrue(orderMatters.Count == 2);
            Assert.IsTrue(orderMatters[0] == "router"); // Route from the Router MUST have fired first
            Assert.IsTrue(orderMatters[1] == "handler"); // Route from the Handler MUST have fired second

            Assert.IsTrue(routeFired, "Route did not fire");
        }

        [TestMethod]
        [TestCategory("Router - PrefixPath")]
        public async Task Router_RoutePath_PrefixNull()
        {
            // Should not explode
            var nullRoutePath = Router.PrefixPath(null, null);
            Assert.IsNull(nullRoutePath);
        }

        [TestMethod]
        [TestCategory("Router - PrefixPath")]
        public async Task Router_RoutePath_PrefixEmptyPath()
        {
            string[] routePath = null;

            // Prepend a prefix to an empty route path
            var result = Router.PrefixPath(routePath, "should do nothing");
            Assert.IsNull(result); // no elements in the path currently, so no prefix is possible
        }

        [TestMethod]
        [TestCategory("Router - PrefixPath")]
        public async Task Router_RoutePath_Prefix()
        {
            string subject = "subject";
            string prefix = "prefix";

            var routePath = new string[] { subject };

            var result = Router.PrefixPath(routePath, prefix);
            Assert.IsTrue(result.Length == 1);
            Assert.IsTrue(result[0] == prefix + subject, "Prefix did not properly prefix");
        }

        [TestMethod]
        [TestCategory("Router - PushPath")]
        public async Task Router_RoutePath_PushNull()
        {
            // Should not explode
            var nullRoutePath = Router.PushPath(null, null);
            Assert.IsNull(nullRoutePath);
        }

        [TestMethod]
        [TestCategory("Router - PushPath")]
        public async Task Router_RoutePath_Push()
        {
            string subject = "subject";
            var routePath = new List<string>().ToArray();

            var result = Router.PushPath(routePath, subject);
            Assert.IsTrue(result.Length == 1, "Should be exactly 1 item in the path");
            Assert.IsTrue(result[0] == subject, $"Item should be '{subject}'");
        }

        [TestMethod]
        [TestCategory("Router - UpdatePath")]
        public async Task Router_RoutePath_UpdateNull()
        {
            var nullRoutePath = Router.UpdatePath(null, null);
            Assert.IsNull(nullRoutePath);
        }

        [TestMethod]
        [TestCategory("Router - UpdatePath")]
        public async Task Router_RoutePath_UpdateEmpty()
        {
            var routePath = new List<String>().ToArray();
            var revisedPath = Router.UpdatePath(routePath, "Should Not Be Added");
            Assert.IsTrue(routePath.Length == 0);
            Assert.IsTrue(revisedPath.Length == 0);
        }

        [TestMethod]
        [TestCategory("Router - UpdatePath")]
        public async Task Router_RoutePath_Update()
        {
            string revised = "revised";
            var routePath = new string[] { "WillBeRemoved" };

            var result = Router.UpdatePath(routePath, revised);
            Assert.IsTrue(result.Length == 1, "Should be exactly 1 item in the path");
            Assert.IsTrue(result[0] == revised, $"Item should be '{revised}'");
        }
    }
}