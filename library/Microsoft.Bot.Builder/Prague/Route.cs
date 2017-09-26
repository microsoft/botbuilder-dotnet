using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Prague
{
    public class Route
    {
        public delegate void RouteAction();

        public Route(RouteAction action)
        {
            this.Action = action ?? throw new ArgumentNullException("action");
        }

        public double Score { get; set; } = 1.0;
        public bool Thrown { get; set; } = false;

        public RouteAction Action { get; internal set; }
    }

    public class Match
    {
        public double Score { get; set; } = 1.0;

    }

    public interface IRouter
    {
        Route GetRoute(IBotContext context);
    }

    public interface IHandler
    {
        void Execute();
    }

    public class NullRouter : IRouter
    {
        public Route GetRoute(IBotContext context)
        {
            return null;
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

    public class IfMatch : IRouter
    {
        public delegate bool Condition(IBotContext context);
        private Condition _condition = null;
        private IRouter _route = null;

        public IfMatch(Condition condition, IRouter route)
        {
            _condition = condition ?? throw new ArgumentNullException("condition");
            _route = route ?? throw new ArgumentNullException("route");
        }

        public Route GetRoute(IBotContext context)
        {
            bool matches = _condition(context);
            if (matches)
            {
                return _route.GetRoute(context);
            }
            else
            {
                return null;
            }
        }
    }

    public class First : IRouter
    {
        IRouter[] _routers;

        public First(params IRouter[] routers)
        {
            _routers = routers;
        }
        public Route GetRoute(IBotContext context)
        {
            if (_routers != null)
            {
                foreach (IRouter router in _routers)
                {
                    if (router != null)
                    {
                        Route r = router.GetRoute(context);
                        if (r != null)
                            return r;
                    }
                }
            }
            return null;
        }
    }

    public class SimpleHandler : IHandler
    {
        Action _action;

        public SimpleHandler(Action action)
        {
            _action = action ?? throw new ArgumentNullException("action");
        }
        public void Execute()
        {
            _action();
        }

        public static SimpleHandler Create(Action a)
        {
            return new SimpleHandler(a);
        }
    }

    public class SimpleRouter : IRouter
    {
        private Route.RouteAction _action;
        public SimpleRouter(Route.RouteAction action)
        {
            _action = action ?? throw new ArgumentNullException("action");
        }

        public Route GetRoute(IBotContext context)
        {
            return new Route(_action);
        }

        public static SimpleRouter Create( Route.RouteAction a )
        {
            return new SimpleRouter(a);
        }
    }

}
