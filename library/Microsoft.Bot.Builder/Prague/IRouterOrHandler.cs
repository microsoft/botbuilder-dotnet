using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Prague
{
    public interface IRouterOrHandler
    {
    }

    public interface IRouter : IRouterOrHandler
    {
        Task<Route> GetRoute(IBotContext context);
    }

    public interface IHandler : IRouterOrHandler
    {
        Task Execute();
    }

    public static class RoutingUtilities
    {
        private static NullRouter _nullRouter = new NullRouter();

        public static async Task RouteMessage(IRouterOrHandler routerOrHandler, IBotContext context)
        {
            Route r = await routerOrHandler.AsRouter().GetRoute(context).ConfigureAwait(false);
            if (r != null)
                await r.Action().ConfigureAwait(false);
            else
                return; 
        }

        public static bool IsRouter(this IRouterOrHandler routerOrHandler)
        {
            return (routerOrHandler is IRouter); // Here for Compat with the JS SDK.            
        }

        public static IRouter AsRouter(this IRouterOrHandler routerOrHandler)
        {
            if (routerOrHandler is IHandler h)
                return new SimpleRouter(h);
            else if (routerOrHandler is IRouter r)
                return r;
            else
                throw new InvalidOperationException($"Unknown RouteHandler Type: '{routerOrHandler.GetType().FullName}'");
        }

        public static NullRouter NullRouter { get { return _nullRouter; } }
    }
}
