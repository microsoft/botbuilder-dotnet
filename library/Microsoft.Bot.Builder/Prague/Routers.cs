using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Prague
{
    public interface IRouter
    {
        Route GetRoute(IBotContext context);
    }

    public abstract class CompoundRouterBase : IRouter
    {
        private List<IRouter> _routers = new List<IRouter>();

        public CompoundRouterBase Add(params IRouter[] routers)
        {
            if (routers == null)
            {
                _routers.Add(new NullRouter());
            }
            else
            {
                foreach (var r in routers)
                {
                    _routers.Add(r ?? new NullRouter());
                }
            }

            return this;
        }

        public CompoundRouterBase Add(params IHandler[] handlers)
        {
            if (handlers == null)
            {
                _routers.Add(new NullRouter());
            }
            else
            {
                foreach (var h in handlers)
                {
                    if (h == null)
                        _routers.Add(new NullRouter());
                    else
                        _routers.Add(new SimpleRouter(h.Execute));
                }
            }

            return this;
        }

        public abstract Route GetRoute(IBotContext context);

        public void Clear()
        {
            _routers.Clear();
        }

        public IList<IRouter> SubRouters { get => _routers; }
    }

    public sealed class NullRouter : IRouter
    {
        public Route GetRoute(IBotContext context)
        {
            return null;
        }
    }

    /// <summary>
    /// Router that throws an InvalidOperationExcpetion when it's used. 
    /// This router is primarly used for Unit Testig to insure routing
    /// order and proper error handling. 
    /// </summary>
    public sealed class ErrorRouter : IRouter
    {
        public Route GetRoute(IBotContext context)
        {            
            throw new InvalidOperationException("Error by design");
        }
    }

    public class AnonymousRouter : IRouter
    {
        private GetRouteDelegate _delegate;
        public delegate Route GetRouteDelegate(IBotContext context);
        public AnonymousRouter(GetRouteDelegate getRouteLambda)
        {
            _delegate = getRouteLambda ?? throw new ArgumentException("getRouteLambda");
        }

        public Route GetRoute(IBotContext context)
        {
            return _delegate(context);
        }
    }

    public class SimpleRouter : IRouter
    {        
        public delegate void ActionWithContext(IBotContext context);

        private ActionWithContext _action;

        public SimpleRouter(ActionWithContext action)
        {
            _action = action ?? throw new ArgumentNullException("action");
        }

        public SimpleRouter(Route.RouteAction action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            _action = (context) => action(); 
        }

        public SimpleRouter(IHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");

            _action = (context) => handler.Execute();
        }

        public Route GetRoute(IBotContext context)
        {
            return new Route(()=> _action(context));
        }

        public static SimpleRouter Create(ActionWithContext a)
        {
            return new SimpleRouter(a);
        }

        public static SimpleRouter Create(Route.RouteAction a)
        {
            return new SimpleRouter(a);
        }

    }

    public class ScoredRouter : IRouter
    {        
        private Route _route;

        public ScoredRouter(Route.RouteAction action, double score)
        {
            _route = new Route(action, score);             
        }

        public Route GetRoute(IBotContext context)
        {
            return _route;
        }

        public static ScoredRouter Create(Route.RouteAction a, double score)
        {
            return new ScoredRouter(a, score);
        }
    }

    public class FirstRouter : CompoundRouterBase
    {
        public FirstRouter(): base()
        { }
        public FirstRouter(params IRouter[] routers)
        {
            this.Add(routers);
        }
        public override Route GetRoute(IBotContext context)
        {
            foreach (IRouter router in this.SubRouters)
            {
                Route r = router.GetRoute(context);
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
        public BestRouter(params IRouter[] routers)
        {
            this.Add(routers);
        }
        public override Route GetRoute(IBotContext context)
        {
            Route best = new MinRoute();
            foreach (var router in this.SubRouters)
            {
                var route = router.GetRoute(context);
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
        public delegate bool Condition(IBotContext context);
        private Condition _condition = null;
        private IRouter _ifMatchesRouter = null;
        private IRouter _elseMatchesRouter = null;

        public IfMatch(Condition condition, IRouter ifMatchesRouter) : this (condition, ifMatchesRouter, new NullRouter())
        {            
        }

        public IfMatch(Condition condition, IRouter ifMatchesRouter, IRouter elseMatchesRouter )
        {
            _condition = condition ?? throw new ArgumentNullException("condition");
            _ifMatchesRouter = ifMatchesRouter ?? throw new ArgumentNullException("ifMatchesRouter");
            _elseMatchesRouter = elseMatchesRouter ?? throw new ArgumentNullException("elseMatchesRouter");
        }

        public Route GetRoute(IBotContext context)
        {
            bool matches = _condition(context);
            if (matches)
            {
                return _ifMatchesRouter.GetRoute(context);
            }
            else
            {
                return _elseMatchesRouter.GetRoute(context);
            }
        }
    }



}
