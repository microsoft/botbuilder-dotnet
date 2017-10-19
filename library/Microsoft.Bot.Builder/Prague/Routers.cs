using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Prague
{       
    public class AnonymousRouter : IRouter
    {
        private readonly Func<IBotContext, Task<Route>> _delegate;
        public AnonymousRouter(Func<IBotContext, Task<Route>> getRouteLambda)
        {
            _delegate = getRouteLambda ?? throw new ArgumentException(nameof(getRouteLambda)); 
        }

        public Task<Route> GetRoute(IBotContext context, string[] routePath = null)
        {
            return _delegate(context);
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

    public class IfMatch : IRouter
    {
        public delegate Task<bool> ConditionAsync(IBotContext context);
        public delegate bool Condition(IBotContext context);

        private ConditionAsync _condition = null;
        private IRouterOrHandler _ifMatchesRouterOrHandler = null;
        private IRouterOrHandler _elseMatchesRouterOrHandler = null;

        public IfMatch(Condition condition, IRouterOrHandler ifMatches, IRouterOrHandler elseMatches = null) 
            : this(async (context) => condition(context), ifMatches, elseMatches)
        {
        }

        public IfMatch(ConditionAsync condition, IRouterOrHandler ifRouterOrHandler, IRouterOrHandler elseRouterOrHandler = null)
        {
            _condition = condition ?? throw new ArgumentNullException(nameof(condition));
            _ifMatchesRouterOrHandler = ifRouterOrHandler ?? throw new ArgumentNullException(nameof(ifRouterOrHandler)); 
            _elseMatchesRouterOrHandler = elseRouterOrHandler ?? Router.NoRouter();
        }

        public async Task<Route> GetRoute(IBotContext context, string[] foo = null)
        {
            bool matches = await _condition(context).ConfigureAwait(false);
            if (matches)
            {
                return await Router.ToRouter(_ifMatchesRouterOrHandler).GetRoute(context).ConfigureAwait(false);
            }
            else
            {
                return await Router.ToRouter(_elseMatchesRouterOrHandler).GetRoute(context).ConfigureAwait(false);
            }
        }
    }
}