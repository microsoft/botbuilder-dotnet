using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Middleware;
using static Microsoft.Bot.Builder.Conversation.Routers;

namespace Microsoft.Bot.Builder.Conversation
{
    public class ActivityRoutingMiddleware : IReceiveActivity
    {
        Router _router;
        
        public ActivityRoutingMiddleware(Router router)
        {
            _router = router ?? throw new ArgumentNullException(nameof(router));
        }

        //public ActivityRoutingMiddleware(Handler pragueHandler)
        //{
        //    if (pragueHandler == null)
        //        throw new ArgumentNullException(nameof(pragueHandler));

        //    _pragueRouter = Router.ToRouter(pragueHandler); 
        //}        

        public async Task ReceiveActivity(IBotContext context, MiddlewareSet.NextDelegate next)
        {
            Route route = await _router.GetRoute(context).ConfigureAwait(false);
            if (route == null) // don't call the next middleware
                return; 

            await route.Action(context, null).ConfigureAwait(false);

            await next().ConfigureAwait(false);
        }
    }
}
