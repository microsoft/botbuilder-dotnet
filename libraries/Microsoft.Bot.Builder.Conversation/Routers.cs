using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Conversation
{
    public static class Routers
    {
        public static Router TryInOrder(params Router[] routers)
        {
            return new Router(async (context, routePath) =>
            {
                Router.PushPath(routePath, "first()");
                if (routers != null)
                {
                    foreach (var router in routers)
                    {
                        Route route = await router.GetRoute(context).ConfigureAwait(false);
                        if (route != null)
                            return route;
                    }
                }

                return null;
            });
        }

        public static Router TryBest(params Router[] routers)
        {
            Router bestRouter = new Router(async (context, routePath) =>
            {
                if (routers == null)
                    return null;

                List<Task<Route>> tasks = new List<Task<Route>>();
                int index = 1;
                foreach (Router router in routers)
                {
                    if (router is null) // Skip any null routers that may be in the list. 
                        continue;

                    string path = $"best ({index++} of {routers.Length})";
                    var revisedPath = Router.PushPath(routePath, path);
                    tasks.Add(router.GetRoute(context, revisedPath));
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

        public static Router Default(Router mainRouter, Func<string, Task<Router>> getDefaultRouter)
        {
            return new Router(async (context, routePath) =>
            {
                var route = await mainRouter.GetRoute(context, routePath).ConfigureAwait(false);
                if (route.Action != null)
                    return route;
                // get default router
                var defaultRouter = await getDefaultRouter(route.Reason).ConfigureAwait(false);
                return await defaultRouter.GetRoute(context, routePath).ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Router that throws an InvalidOperationExcpetion when it's used. 
        /// This router is primarly used for Unit Testing to insure routing
        /// order and proper error handling. 
        /// </summary>
        public static Router Error()
        {
            Router errorRouter = new Router((context, routePath) =>
            {
                throw new InvalidOperationException("Error by design");
            });

            return errorRouter;
        }

        /// <summary>
        /// If the Router evaluates to a non-nullRoute, then when the actual route
        /// is executed, execute the firstDo() before doing the Do() for the route. 
        /// </summary>
        public static Router DoBefore(Router router, Func<IBotContext, object, Task> firstDo)
        {
            return new Router(async (context, routePath) =>
            {
                Router.PushPath(routePath, $"DoBefore({firstDo.GetType().Name})");
                Route route = await router.GetRoute(context, routePath).ConfigureAwait(false);
                if (route != null)
                {
                    var thenDo = route.Action;
                    route.Action = async (ctx, result) =>
                    {
                        await firstDo(ctx, result).ConfigureAwait(false);
                        await thenDo(ctx, result).ConfigureAwait(false);
                    };
                }
                return route;
            });
        }

        /// <summary>
        /// If the router evaluates to non-null route, then when the actual route
        /// is executed, execute the Do() before calling the thenDo()
        /// </summary>
        /// <param name="thenDo"></param>
        /// <returns></returns>
        public static Router DoAfter(Router router, Func<IBotContext, object, Task> thenDo)
        {
            return new Router(async (context, routePath) =>
            {
                Router.PushPath(routePath, $"DoAfter({thenDo.GetType().Name})");
                Route route = await router.GetRoute(context, routePath).ConfigureAwait(false);
                if (route != null)
                {
                    var action = route.Action;
                    route.Action = async (ctx, result) =>
                    {
                        await action(ctx, result).ConfigureAwait(false);
                        await thenDo(ctx, result).ConfigureAwait(false);
                    };
                }
                return route;
            });
        }


        public static Router Simple(Func<IBotContext, MatcherResult, Task> asyncAction)
        {
            Router r = new Router(new Route(asyncAction));
            return r;
        }

        public static Router Simple(Action<IBotContext, MatcherResult> action)
        {
            return new Router(new Route((context, matchResult) => Task.Run(() => action(context, matchResult))));
        }

        public static Router Scored(Func<IBotContext, MatcherResult, Task> asyncAction, double score)
        {            
            return new Router(new Route(asyncAction, score));
        }

        public static Router Scored(Action<IBotContext, MatcherResult> action, double score)
        {
            return new Router(new Route((context, matchResult) => Task.Run(() => action(context, matchResult)), score));
        }

    }
}