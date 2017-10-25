using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Conversation
{
    //public abstract class RouterOrHandler
    //{
    //    public static implicit operator RouterOrHandler(Action a)
    //    {
    //        return new Handler(a);
    //    }
    //}

    public class Router : IMiddleware, IReceiveActivity
    {
        // public delegate Task<Route> GetRouteDelegate(IBotContext context, string[] routePath=null);

        private Func<IBotContext, string[], Task<Route>> _getRoute;

        public Router(Func<IBotContext, string[], Task<Route>> getRoute)
        {
            _getRoute = getRoute ?? throw new ArgumentNullException(nameof(getRoute));
        }

        public Router(Route route)
        {
            _getRoute = (context, routePath) => Task.FromResult(route ?? throw new ArgumentException(nameof(route)));
        }

        /// <summary>
        /// Get the route from the current router
        /// </summary>
        /// <param name="context"></param>
        /// <param name="routePath"></param>
        /// <returns></returns>
        public Task<Route> GetRoute(IBotContext context, string[] routePath = null)
        {
            return _getRoute(context, routePath);
        }

        /// <summary>
        /// Route the context routePath
        /// </summary>
        /// <param name="context"></param>
        /// <param name="routePath"></param>
        /// <returns></returns>
        public Task<Route> Route(IBotContext context, string[] routePath = null)
        {
            return this.GetRoute(context, routePath);
        }

        //public Router DefaultDo(Func<Task> handler)
        //{
        //    return this.DefaultTry(ReadOnlyCollectionBuilder => new )
        //}

        /// <summary>
        /// If the Router evaluates to a non-nullRoute, then when the actual route
        /// is executed, execute the firstDo() before doing the Do() for the route. 
        /// </summary>
        public Router DoBefore(Func<IBotContext, object, Task> firstDo)
        {
            return Routers.DoBefore(this, firstDo);
        }

        /// <summary>
        /// If the router evaluates to non-null route, then when the actual route
        /// is executed, execute the Do() before calling the thenDo()
        /// </summary>
        /// <param name="thenDo"></param>
        /// <returns></returns>
        public Router DoAfter(Func<IBotContext, object, Task> thenDo)
        {
            return Routers.DoAfter(this, thenDo);
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


        /// <summary>
        /// Middleware ReceiveActivity handler which routes to the router
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<ReceiveResponse> ReceiveActivity(BotContext context)
        {
            Route route = await GetRoute(context, new string[] { "ReceiveActivity" }).ConfigureAwait(false);
            if (route != null)
            {
                await route.Action(context, null).ConfigureAwait(false);
                return new ReceiveResponse(true);
            }
            else
            {
                return new ReceiveResponse(false);
            }
        }

    }
}
