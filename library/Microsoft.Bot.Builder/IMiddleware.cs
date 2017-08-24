using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using System.Net;

namespace Microsoft.Bot.Builder
{
    public interface IMiddleware
    {

    }

    /// <summary>
    ///  By default, most activities will be dispatched and an HTTP status code of 202 will be 
    ///  sent to the calling service upon the successful completion of the handler the activity
    ///  was dispatched to.Handlers can customize the response returned to the calling service
    ///  by resolving a promise with a RouteResponse object. For example, "invoke" activities can
    ///  specify the body that should be returned to the service using this technique.
    /// </summary>
    public class ReceiveResponse
    {
        public ReceiveResponse()
        { }

        public ReceiveResponse(bool handled)
        {
            this.Handled = handled;
        }

        /// <summary>
        /// If true the received activity was dispatched to a handler. 
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// (OPTIONAL) HTTP status code to return. 
        /// </summary>
        public HttpStatusCode? Status { get; set; }

        /// <summary>
        /// (OPTIONAL) body to return with the response. 
        /// </summary>
        public object Body { get; set; }
    }


    public interface IContextCreated : IMiddleware
    {
        Task ContextCreated(BotContext context, CancellationToken token);
    }

    public interface IReceiveActivity : IMiddleware
    {
        Task<ReceiveResponse> ReceiveActivity(BotContext context, CancellationToken token);
    }

    public interface IPostActivity : IMiddleware
    {
        Task PostActivity(BotContext context, IList<IActivity> activities, CancellationToken token);
    }

    public interface IContextDone : IMiddleware
    {
        Task ContextDone(BotContext context, CancellationToken token);
    }

    public static partial class MiddlewareExtensions
    {
        public static IEnumerable<T> Where<T>(this IList<IMiddleware> middlewares) where T : IMiddleware
        {
            return middlewares.Where(x => x is T).Cast<T>();
        }
    }
}
