using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Bot.Builder.Prague;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;

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

            IRouter router = new Router(
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

            IRouter router = new Router(
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

            SimpleHandler one = new SimpleHandler(() => orderMatters.Add("one"));
            SimpleHandler two = new SimpleHandler(() => orderMatters.Add("two"));

            IRouter foo = Router.DoBefore(one, two);
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

            SimpleHandler one = new SimpleHandler(() => orderMatters.Add("one"));
            IRouter nullRouter = new Router(
               async (context) =>
               {
                   routerFired = true;

                   // returning a null route here means DoBefore rule should not run
                   // If an actual Route was returned, then the Before would be triggered
                   return null;
               }
           );

            IRouter foo = Router.DoBefore(one, nullRouter);
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
            SimpleHandler one = new SimpleHandler(() => orderMatters.Add("handler"));

            bool routerFired = false;
            bool routeFired = false;

            IRouter router = new Router(
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

            IRouter foo = Router.DoBefore(one, router);
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

            SimpleHandler one = new SimpleHandler(() => orderMatters.Add("one"));
            SimpleHandler two = new SimpleHandler(() => orderMatters.Add("two"));

            IRouter foo = Router.DoAfter(one, two);
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
            IList<string> routingPath = new List<string>();

            SimpleHandler one = new SimpleHandler(() => orderMatters.Add("one"));
            SimpleHandler two = new SimpleHandler(() => orderMatters.Add("two"));

            IRouter foo = Router.DoAfter(one, two);
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

            SimpleHandler one = new SimpleHandler(() => orderMatters.Add("one"));
            IRouter nullRouter = new Router(
               async (context) =>
               {
                   routerFired = true;

                   // returning a null route here means DoBefore rule should not run
                   // If an actual Route was returned, then the Before would be triggered
                   return null;
               }
           );

            IRouter foo = Router.DoAfter(nullRouter, one);
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
            SimpleHandler one = new SimpleHandler(() => orderMatters.Add("handler"));

            bool routerFired = false;
            bool routeFired = false;

            IRouter router = new Router(
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

            IRouter foo = Router.DoAfter(router, one);
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
            IList<string> routePath = new List<string>();

            // Prepend a prefix to an empty route path
            routePath = Router.PrefixPath(routePath, "should do nothing");
            Assert.IsTrue(routePath.Count == 0); // no elements in the path currently, so no prefix is possible
        }

        [TestMethod]
        [TestCategory("Router - PrefixPath")]
        public async Task Router_RoutePath_Prefix()
        {
            string subject = "subject";
            string prefix = "prefix";

            var routePath = new List<string> { subject };

            Router.PrefixPath(routePath, prefix);
            Assert.IsTrue(routePath.Count == 1);
            Assert.IsTrue(routePath[0] == prefix + subject, "Prefix did not properly prefix");
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
            IList<string> routePath = new List<string>();
            
            routePath = Router.PushPath(routePath, subject);
            Assert.IsTrue(routePath.Count == 1, "Should be exactly 1 item in the path");
            Assert.IsTrue(routePath[0] == subject, $"Item should be '{subject}'");
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
            IList<string> routePath = new List<String>();
            var revisedPath = Router.UpdatePath(routePath, "Should Not Be Added");
            Assert.IsTrue(routePath.Count == 0);
            Assert.IsTrue(revisedPath.Count == 0);
        }

        [TestMethod]
        [TestCategory("Router - UpdatePath")]
        public async Task Router_RoutePath_Update()
        {            
            string revised = "revised";
            IList<string> routePath = new List<string> { "WillBeRemoved" };

            routePath = Router.UpdatePath(routePath, revised); 
            Assert.IsTrue(routePath.Count == 1, "Should be exactly 1 item in the path");
            Assert.IsTrue(routePath[0] == revised, $"Item should be '{revised}'");
        }
    }
}