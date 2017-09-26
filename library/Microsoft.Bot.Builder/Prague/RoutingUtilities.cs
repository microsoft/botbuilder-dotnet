using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Prague
{
    public static class RoutingUtilities
    {
        private static NullRouter _nullRouter = new NullRouter();

        public static void RouteMessage(IRouter router, IBotContext context)
        {
            Route r = router.GetRoute(context);
            if (r != null)
                r.Action();
        }
        public static bool IsRouter(IRouter router)
        {
            // Here for Compat with the JS SDK.
            return true;
        }
        public static bool IsRouter(IHandler handler)
        {
            return (handler is IRouter);
        }

        #region "Helpers around the RouterOrHandler Metaphore in JS and Python"
        public static IRouter ToRouter(IRouter router)
        {
            return router;
        }

        public static IRouter ToRouter(IHandler handler)
        {
            return new SimpleRouter(handler.Execute);
        }
        #endregion

        public static NullRouter NullRouter { get { return _nullRouter; } }

    }
}
