using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Prague
{
    public abstract class CompoundRouterBase : IRouter
    {
        private readonly List<IRouterOrHandler> _routerOrHandler = new List<IRouterOrHandler>();

        public CompoundRouterBase Add(params IRouterOrHandler[] routerOrHandlers)
        {
            if (routerOrHandlers == null)
            {
                _routerOrHandler.Add(new NullRouter());
            }
            else
            {
                foreach (IRouterOrHandler item in routerOrHandlers)
                {
                    if (item == null)
                        _routerOrHandler.Add(new NullRouter());
                    else
                        _routerOrHandler.Add(item);
                }
            }

            return this;
        }

        public abstract Task<Route> GetRoute(IBotContext context, IList<string> foo = null);

        public void Clear()
        {
            _routerOrHandler.Clear();
        }

        public IList<IRouterOrHandler> SubRouters { get => _routerOrHandler; }
    }

    public sealed class NullRouter : IRouter
    {
        public Task<Route> GetRoute(IBotContext context, IList<string> foo = null)
        {
            return Task.FromResult<Route>(null);
        }
    }

    /// <summary>
    /// Router that throws an InvalidOperationExcpetion when it's used. 
    /// This router is primarly used for Unit Testing to insure routing
    /// order and proper error handling. 
    /// </summary>
    public sealed class ErrorRouter : IRouter
    {
        public Task<Route> GetRoute(IBotContext context, IList<string> foo = null)
        {            
            return Task.FromException<Route>(new InvalidOperationException("Error by design"));
        }
    }

    public class AnonymousRouter : IRouter
    {
        private readonly Func<IBotContext, Task<Route>> _delegate;
        public AnonymousRouter(Func<IBotContext, Task<Route>> getRouteLambda)
        {
            _delegate = getRouteLambda ?? throw new ArgumentException(nameof(getRouteLambda)); 
        }

        public Task<Route> GetRoute(IBotContext context, IList<string> foo = null)
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

        public async Task<Route> GetRoute(IBotContext context, IList<string> foo = null)
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

        public async Task<Route> GetRoute(IBotContext context, IList<string> foo = null)
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

    public class FirstRouter : CompoundRouterBase
    {
        public FirstRouter() : base()
        { }
        public FirstRouter(params IRouterOrHandler[] routerOrHandlers)
        {
            this.Add(routerOrHandlers);
        }
        public async override Task<Route> GetRoute(IBotContext context, IList<string> foo = null)
        {
            foreach (IRouterOrHandler rh in this.SubRouters)
            {
                Route r = await rh.AsRouter().GetRoute(context).ConfigureAwait(false);
                if (r != null)
                    return r;
            }

            return null;
        }
    }

    public class BestRouter : CompoundRouterBase
    {
        public BestRouter() : base()
        {
        }
        public BestRouter(params IRouterOrHandler[] routerOrHandlers)
        {
            this.Add(routerOrHandlers);
        }
        public async override Task<Route> GetRoute(IBotContext context, IList<string> foo = null)
        {
            List<Task<Route>> tasks = new List<Task<Route>>();
            foreach (IRouterOrHandler rh in this.SubRouters)
            {
                tasks.Add(rh.AsRouter().GetRoute(context));
            }

            var routes = await Task.WhenAll(tasks).ConfigureAwait(false);

            Route best = new MinRoute();
            foreach (var route in routes)
            {
                if (route != null)
                {
                    if (route.Score >= 1.0)
                        return route;
                    if (route.Score > best.Score)
                        best = route;
                }
            }

            if (best.Score > 0.0 && (!(best is MinRoute)))
                return best;
            else
                return null;
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
            _elseMatchesRouterOrHandler = elseRouterOrHandler ?? new NullRouter();
        }

        public async Task<Route> GetRoute(IBotContext context, IList<string> foo = null)
        {
            bool matches = await _condition(context).ConfigureAwait(false);
            if (matches)
            {
                return await _ifMatchesRouterOrHandler.AsRouter().GetRoute(context).ConfigureAwait(false);
            }
            else
            {
                return await _elseMatchesRouterOrHandler.AsRouter().GetRoute(context).ConfigureAwait(false);
            }
        }
    }
}