using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Prague
{
    public static class RoutingRules
    {        
        public static Router First(params RouterOrHandler[] routerOrHandlers)
        {
            Router firstRouter = new Router(async (context, routePath) =>
            {
                Router.PushPath(routePath, "first()");
                if (routerOrHandlers != null)
                {
                    foreach (RouterOrHandler rh in routerOrHandlers)
                    {
                        Router r = Router.ToRouter(rh);
                        Route route = await r.GetRoute(context).ConfigureAwait(false);
                        if (route != null)
                            return route;
                    }
                }

                return null;
            });

            return firstRouter;
        }

        public static Router Best(params RouterOrHandler[] routerOrHandler)
        {
            Router bestRouter = new Router(async (context, routePath) =>
            {
                if (routerOrHandler == null)
                    return null;

                List<Task<Route>> tasks = new List<Task<Route>>();
                int index = 1;
                foreach (RouterOrHandler rh in routerOrHandler)
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

        public static Router IfTrue(ConditionAsync condition, RouterOrHandler thenDo, RouterOrHandler elseDo = null)
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));

            Router thenRouter = Router.ToRouter(thenDo);
            Router elseRouter = Router.ToRouter(elseDo);

            Router ifTrueRouter = new Router(async (context, routePath) =>
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

        public static Router IfTrue(Condition condition, RouterOrHandler thenDo, RouterOrHandler elseDo = null)
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));

            return IfTrue(async (context) => condition(context), thenDo, elseDo);
        }
    }
}
