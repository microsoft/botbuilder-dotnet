using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Prague
{
    public static class Routers
    {
        /// <summary>
        /// Router that throws an InvalidOperationExcpetion when it's used. 
        /// This router is primarly used for Unit Testing to insure routing
        /// order and proper error handling. 
        /// </summary>
        public static IRouter Error()
        {
            Router errorRouter = new Router(async (context, routePath) =>
            {
                throw new InvalidOperationException("Error by design");
            });

            return errorRouter;
        }

        public static IHandler Simple(Action action)
        {
            return new SimpleHandler(action);
        }

        public static IRouter Simple(Action<IBotContext> action)
        {
            return new SimpleRouter(action);
        }
    }

    public class SimpleRouter : IRouter
    {
        private readonly Func<IBotContext, Task> _action;

        public SimpleRouter(Func<IBotContext, Task> function)
        {
            _action = function ?? throw new ArgumentNullException(nameof(function)); 
        }

        public SimpleRouter(Func<Task> function)
        {
            if (function == null)
                throw new ArgumentNullException(nameof(function));

            _action = (context) => function();
        }

        public SimpleRouter(Action<IBotContext> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action)); 

            _action = async (context) => action(context);
        }

        public SimpleRouter(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            _action = async (context) => action();
        }

        public SimpleRouter(IHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler)); 

            _action = (context) => handler.Execute();
        }

        public async Task<Route> GetRoute(IBotContext context, string[] routePath = null)
        {
            return new Route(() => _action(context));
        }

        public static SimpleRouter Create(Func<IBotContext, Task> a)
        {
            return new SimpleRouter(a);
        }

        public static SimpleRouter Create(Func<Task> a)
        {
            return new SimpleRouter(a);
        }

        public static SimpleRouter Create(Action a)
        {
            return new SimpleRouter(a);
        }
    }

    public class ScoredRouter : IRouter
    {
        private readonly Route _route;

        public ScoredRouter(Func<Task> action, double score)
        {
            _route = new Route(action, score);
        }

        public ScoredRouter(Action action, double score) : this(async () => action(), score)
        {
        }

        public async Task<Route> GetRoute(IBotContext context, string[] foo = null)
        {
            return _route;
        }

        public static ScoredRouter Create(Action a, double score)
        {
            return new ScoredRouter(a, score);
        }
        public static ScoredRouter Create(Func<Task> a, double score)
        {
            return new ScoredRouter(a, score);
        }
    }     
}