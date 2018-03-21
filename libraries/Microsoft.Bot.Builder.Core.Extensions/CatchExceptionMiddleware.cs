using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Core.Extensions
{
    /// <summary>
    /// This piece of middleware can be added to allow you to handle exceptions when they are thrown
    /// within your bot's code or middleware further down the pipeline. Using this handler you might 
    /// send an appropriate message to the user to let them know that something has gone wrong.
    /// You can specify the type of exception the middleware should catch and this middleware can be added
    /// multiple times to allow you to handle different exception types in different ways.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the exception that you want to catch. This can be 'Exception' to
    /// catch all or a specific type of exception
    /// </typeparam>
    public class CatchExceptionMiddleware<T> : IMiddleware where T : Exception
    {
        private readonly Func<ITurnContext, T, Task> _handler;

        public CatchExceptionMiddleware(Func<ITurnContext, T, Task> callOnException)
        {
            _handler = callOnException;
        }

        public async Task OnProcessRequest(ITurnContext context, MiddlewareSet.NextDelegate next)
        {
            try
            {
                // Continue to route the activity through the pipeline
                // any errors further down the pipeline will be caught by
                // this try / catch
                await next().ConfigureAwait(false);
            }
            catch (T ex)
            {
                // If an error is thrown and the exception is of type T then invoke the handler
                await _handler.Invoke(context, ex).ConfigureAwait(false);
            }
        }
    }
}
