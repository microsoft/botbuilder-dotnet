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
                // Continue to route the request through the pipeline
                // any errors further down the pipeline will be caught by
                // this try / catch
                await next();
            }
            catch (T ex)
            {
                // If an error is thrown and the exception is of type T then invoke the handler
                await _handler.Invoke(context, ex);
            }
        }
    }
}
