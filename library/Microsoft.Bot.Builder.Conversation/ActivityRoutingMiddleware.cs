using System;
using System.Threading.Tasks;
using static Microsoft.Bot.Builder.Conversation.Routers;

namespace Microsoft.Bot.Builder.Conversation
{
    public class ActivityRoutingMiddleware : IMiddleware, IReceiveActivity
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

        public async Task<ReceiveResponse> ReceiveActivity(BotContext context)
        {
            Route route = await _router.GetRoute(context).ConfigureAwait(false); 
            if (route == null)
                return new ReceiveResponse(false);

            await route.Action(context, null).ConfigureAwait(false);

            return new ReceiveResponse(true); 
        }
    }
}
