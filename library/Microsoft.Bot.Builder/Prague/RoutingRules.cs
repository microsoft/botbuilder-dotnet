using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Prague
{
    public static class RoutingRules
    {
        /// <summary>
        /// Router that throws an InvalidOperationExcpetion when it's used. 
        /// This router is primarly used for Unit Testing to insure routing
        /// order and proper error handling. 
        /// </summary>
        public static IRouter Error()
        {
            Router errorRouter = new Router(async (context, routePath) =>
            {
                throw new InvalidOperationException("Error by design");
            });

            return errorRouter;
        }

        public static IRouter First(params IRouterOrHandler[] routerOrHandlers)
        {
            Router firstRouter = new Router(async (context, routePath) =>
            {
                Router.PushPath(routePath, "first()");
                if (routerOrHandlers != null)
                {
                    foreach (IRouterOrHandler rh in routerOrHandlers)
                    {
                        IRouter r = Router.ToRouter(rh);
                        Route route = await r.GetRoute(context).ConfigureAwait(false);
                        if (route != null)
                            return route;
                    }
                }

                return null;
            });

            return firstRouter;
        }

        public static IRouter Best(params IRouterOrHandler[] routerOrHandler)
        {
            Router bestRouter = new Router(async (context, routePath) =>
            {
                if (routerOrHandler == null)
                    return null;

                List<Task<Route>> tasks = new List<Task<Route>>();
                int index = 1;
                foreach (IRouterOrHandler rh in routerOrHandler)
                {
                    if (rh is null) // Skip any null routers that may be in the list. 
                        continue;

                    string path = $"best ({index++} of {routerOrHandler.Length})";
                    var revisedPath = Router.PushPath(routePath, path);
                    tasks.Add(Router.ToRouter(rh).GetRoute(context, revisedPath));
                }

                var routes = await Task.WhenAll(tasks).ConfigureAwait(false);

                Route best = null;
                foreach (var route in routes)
                {
                    if (route != null)
                    {
                        if (route.Score >= 1.0)
                            return route;
                        if (best == null || route.Score > best.Score)
                            best = route;
                    }
                }
                return best;
            });

            return bestRouter;
        }

        public delegate Task<bool> ConditionAsync(IBotContext context);
        public delegate bool Condition(IBotContext context);

        public static IRouter IfTrue(ConditionAsync condition, IRouterOrHandler thenDo, IRouterOrHandler elseDo = null)
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));

            IRouter thenRouter = Router.ToRouter(thenDo);
            IRouter elseRouter = Router.ToRouter(elseDo);

            IRouter ifTrueRouter = new Router(async (context, routePath) =>
            {
                routePath = Router.PushPath(routePath, $"ifTrue({condition.GetType().Name})");
                bool result = await condition(context).ConfigureAwait(false);
                if (result)
                {
                    Route thenRoute = await thenRouter.GetRoute(context, Router.PrefixPath(routePath, "THEN for")).ConfigureAwait(false);
                    return thenRoute;
                }
                else
                {
                    Route elseRoute = await elseRouter.GetRoute(context, Router.PrefixPath(routePath, "ELSE for")).ConfigureAwait(false);
                    return elseRoute;
                }
            });

            return ifTrueRouter;
        }

        public static IRouter IfTrue(Condition condition, IRouterOrHandler thenDo, IRouterOrHandler elseDo = null)
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));

            return IfTrue(async (context) => condition(context), thenDo, elseDo);
        }
    }
}
