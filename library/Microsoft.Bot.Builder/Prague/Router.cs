using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Prague
{
    public class Router : IRouter, IMiddleware, IReceiveActivity
    {
        public delegate Task<Route> GetRouteDelegate(IBotContext context);
        public delegate Task<Route> GetRouteDelegateRoutePath(IBotContext context, string[] routePath);

        private GetRouteDelegateRoutePath _getRoute;

        public Router(GetRouteDelegate d) : this((context, routePath) => d(context))
        {
        }

        public Router(GetRouteDelegateRoutePath d)
        {
            _getRoute = d ?? throw new ArgumentNullException(nameof(d));
        }

        public Task<Route> GetRoute(IBotContext context, string[] routePath = null)
        {
            return _getRoute(context, routePath);
        }

        public async Task<ReceiveResponse> Route(IBotContext context, string[] routePath = null)
        {
            Route r = await GetRoute(context, routePath).ConfigureAwait(false);
            if (r != null)
            {
                await r.Action();
                return new ReceiveResponse(true);
            }
            else
            {
                return new ReceiveResponse(false);
            }
        }

        public Task<ReceiveResponse> ReceiveActivity(BotContext context)
        {
            return Route(context);
        }

        public static IRouter DoHandler(IHandler handler)
        {
            return ToRouter(handler);
        }

        public static IRouter NoRouter()
        {
            return new Router((context) => Task.FromResult<Route>(null));
        }

        /// <summary>
        /// If the "ThenDo()" evaluates to a non-nullRoute, then when the actual route
        /// is fired, execute the firstDo() before doing the thenDo(). 
        /// </summary>
        public static IRouter DoBefore(IHandler firstDo, IRouterOrHandler thenDo)
        {
            IRouter thenRouter = ToRouter(thenDo);

            Router r = new Router(async (context, routePath) =>
               {
                   Router.PushPath(routePath, $"DoBefore({firstDo.GetType().Name})");
                   Route result = await thenRouter.GetRoute(context, routePath).ConfigureAwait(false);
                   if (result != null)
                   {
                       var originalAction = result.Action;
                       result.Action = async () =>
                       {
                           await firstDo.Execute().ConfigureAwait(false);
                           await originalAction().ConfigureAwait(false);
                       };
                   }
                   return result;
               });

            return r;
        }

        public static IRouter DoAfter(IRouterOrHandler firstDo, IHandler thenDo)
        {
            IRouter firstRouter = ToRouter(firstDo);

            Router r = new Router(async (context, routePath) =>
            {
                Router.PushPath(routePath, $"DoAfter({thenDo.GetType().Name})");
                Route result = await firstRouter.GetRoute(context, routePath).ConfigureAwait(false);
                if (result != null)
                {
                    var originalAction = result.Action;
                    result.Action = async () =>
                    {
                        await originalAction().ConfigureAwait(false);
                        await thenDo.Execute().ConfigureAwait(false);
                    };
                }
                return result;
            });

            return r;
        }

        /// <summary>
        /// Adds a prefix to the "top" item in the path to make it more readable for debugging. For example
        /// Router.PrefixPath(path, "THEN for")
        /// </summary>
        public static string[] PrefixPath(string[] routePath, string prefix)
        {
            if (routePath == null)
                return null;

            List<string> rp = new List<string>(routePath); 
            int index = rp.Count - 1;
            rp[index] = prefix + rp[index];
            return rp.ToArray();
        }

        /// <summary>
        /// Pushes a new item into the Path stack. 
        /// Note: Can't pass in an ArrayList here, as we do NOT want to 
        /// modify the original. Need to make a new copy for the caller to consume.
        /// </summary>
        public static string[] PushPath(string[] routePath, string path)
        {
            if (routePath == null)
                return null;

            List<string> rp = new List<string>(routePath);
            rp.Add(path);

            return rp.ToArray();
        }

        public static string[] UpdatePath(string[] routePath, string entry)
        {
            if (routePath == null)
                return null;

            if (routePath.Length == 0)
                return new string[0];

            List<string> rp = new List<string>(routePath);
            rp[rp.Count - 1] = entry;            

            return rp.ToArray();
        }

        public static IRouter ToRouter(IRouterOrHandler routerOrHandler)
        {
            if (routerOrHandler is IHandler h)
                return new Router(async (context) => new Route(h.Execute, 1.0));
            else if (routerOrHandler is IRouter r)
                return r;
            else if (routerOrHandler is null)
                return NoRouter(); 
            else
                throw new InvalidOperationException($"Unknown RouteHandler Type: '{routerOrHandler.GetType().FullName}'");
        }
    }
}
