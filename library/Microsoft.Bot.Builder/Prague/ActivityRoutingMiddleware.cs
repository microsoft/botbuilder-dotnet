using System;
using System.Threading.Tasks;
using static Microsoft.Bot.Builder.Prague.Routers;

namespace Microsoft.Bot.Builder.Prague
{
    public class ActivityRoutingMiddleware : IMiddleware, IReceiveActivity
    {
        Router _pragueRouter;
        
        public ActivityRoutingMiddleware(Router pragueRouter)
        {
            _pragueRouter = pragueRouter ?? throw new ArgumentNullException(nameof(pragueRouter));
        }

        public ActivityRoutingMiddleware(Handler pragueHandler)
        {
            if (pragueHandler == null)
                throw new ArgumentNullException(nameof(pragueHandler));

            _pragueRouter = Router.ToRouter(pragueHandler); 
        }

        public async Task<ReceiveResponse> ReceiveActivity(BotContext context)
        {
            Route r = await _pragueRouter.GetRoute(context).ConfigureAwait(false); 
            if (r == null)
                return new ReceiveResponse(false);

            await r.Action().ConfigureAwait(false);

            return new ReceiveResponse(true); 
        }
    }
}
