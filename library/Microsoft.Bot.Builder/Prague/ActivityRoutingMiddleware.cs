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
            _pragueRouter = pragueRouter ?? throw new ArgumentNullException(nameof(pragueRouter));
        }

        public ActivityRoutingMiddleware(IHandler pragueHandler)
        {
            if (pragueHandler == null)
                throw new ArgumentNullException(nameof(pragueHandler)); 

            _pragueRouter = new SimpleRouter(pragueHandler.Execute);
        }

        public async Task<ReceiveResponse> ReceiveActivity(BotContext context, CancellationToken token)
        {
            Route r = await _pragueRouter.GetRoute(context).ConfigureAwait(false); 
            if (r == null)
                return new ReceiveResponse(false);

            await r.Action().ConfigureAwait(false);

            return new ReceiveResponse(true); 
        }
    }
}
