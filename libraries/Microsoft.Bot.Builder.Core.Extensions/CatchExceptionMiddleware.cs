using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Core.Extensions
{
    public class CatchExceptionMiddleware<T> : IMiddleware where T : Exception
    {
        private readonly Func<IBotContext, T, Task> _handler;

        public CatchExceptionMiddleware(Func<IBotContext, T, Task> callOnException)
        {
            _handler = callOnException;
        }

        public async Task OnProcessRequest(IBotContext context, MiddlewareSet.NextDelegate next)
        {
            try
            {
                await next();
            }
            catch (T ex)
            {
                await _handler.Invoke(context, ex);
            }
        }
    }
}
