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
        Task<Route> GetRoute(IBotContext context, String[] routePath = null);
    }

    public interface IHandler : IRouterOrHandler
    {
        Task Execute();
    }

    public static class RoutingUtilities
    {        
        public static async Task RouteMessage(IRouterOrHandler routerOrHandler, IBotContext context)
        {
            Route r = await Router.ToRouter(routerOrHandler).GetRoute(context).ConfigureAwait(false);
            if (r != null)
                await r.Action().ConfigureAwait(false);
            else
                return; 
        }        
    }
}
