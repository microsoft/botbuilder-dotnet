using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Core.Extensions
{
    public class CatchExceptionMiddleware : IMiddleware
    {
        private readonly CatchExceptionHandler _handler;

        public CatchExceptionMiddleware(CatchExceptionHandler handler)
        {
            _handler = handler;
        }

        public async Task OnProcessRequest(IBotContext context, MiddlewareSet.NextDelegate next)
        {
            await CatchError(context, next);
        }

        private async Task CatchError(IBotContext context, MiddlewareSet.NextDelegate next)
        {
            try
            {
                await next();
            }
            catch (Exception ex)
            {
                await _handler.Invoke(context, ex);
            }
        }

        public delegate Task CatchExceptionHandler(IBotContext context, Exception exception);
    }
}
