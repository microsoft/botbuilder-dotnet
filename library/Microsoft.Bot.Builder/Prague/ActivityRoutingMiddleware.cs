using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Prague
{
    public class ActivityRoutingMiddleware : IMiddleware, IReceiveActivity
    {
        IRouter _pragueRouter;
        
        public ActivityRoutingMiddleware(IRouter pragueRouter)
        {
            _pragueRouter = pragueRouter ?? throw new ArgumentNullException("pragueRouter");
        }

        public ActivityRoutingMiddleware(IHandler pragueHandler)
        {
            if (pragueHandler == null)
                throw new ArgumentNullException("pragueHandler");

            _pragueRouter = new SimpleRouter(pragueHandler.Execute);
        }

        public async Task<ReceiveResponse> ReceiveActivity(BotContext context, CancellationToken token)
        {
            Route r = _pragueRouter.GetRoute(context);
            if (r == null)
                return new ReceiveResponse(false);

            r.Action();

            return new ReceiveResponse(true); 
        }
    }
}
