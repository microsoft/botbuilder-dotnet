using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Conversation
{
    public class MatcherResult
    {
        public object Result { get; set; }
        public double? Score { get; set; }
    }

    // Func<IBotContext, MatcherResult<T>>
    public delegate MatcherResult Matcher(IBotContext context);

    public class IfMatches
    {
        private Func<IBotContext, Task<MatcherResult>> _matcher;

        public IfMatches(Func<IBotContext, Task<MatcherResult>> matcher)
        {
            this._matcher = matcher;
        }

        public Router ThenDo(Func<IBotContext, MatcherResult, Task> thenHandler)
        {
            return this.ThenTry((result) => new Router(new Route(thenHandler)));
        }

        public Router ThenTry(Router router)
        {
            return new IfMatchesThen(this._matcher, router);
        }

        public Router ThenTry(Func<MatcherResult, Router> getThenRouter)
        {
            return new IfMatchesThen(this._matcher, getThenRouter);
        }
    }


    public class IfMatchesThen : Router
    {
        public IfMatchesThen(Func<IBotContext, Task<MatcherResult>> matcher,
            Func<MatcherResult, Router> getThenRouter)
            : base(async (context, routePath) =>
            {
                var result = await matcher(context).ConfigureAwait(false);
                if (result.Result != null)
                    return await getThenRouter(result).GetRoute(context, routePath).ConfigureAwait(false);
                else
                    return null;
            })
        {
        }

        public IfMatchesThen(Func<IBotContext, Task<MatcherResult>> matcher, Router router)
            : base(async (context, routePath) =>
            {
                var result = await matcher(context).ConfigureAwait(false);
                if (result.Result != null)
                    return await router.GetRoute(context, routePath).ConfigureAwait(false);
                else
                    return null;
            })
        {
        }

    }

    public class IfTrue : IfMatches
    {
        public IfTrue(Func<IBotContext, Task<MatcherResult>> predicate)
            : base((ctx) => predicate(ctx))
        {
        }
    }

    //public class IfMatchesElse<ResultT> : Router
    //{
    //    public IfMatchesElse(Func<IBotContext, MatcherResult<ResultT>> matcher,
    //        Func<MatcherResult<ResultT>, Router> getThenRouter,
    //        Func<MatcherResult<ResultT>, Router> getElseRouter)
    //        : base((context, paths) => {
    //            var result = matcher(context);
    //            if (result.Result != null)
    //                return getThenRouter(result).GetRoute(context, paths);
    //            else
    //                return getElseRouter(result).GetRoute(context, paths);
    //        })
    //    {
    //    }

    //}

}
