using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Conversation
{
    public static class RoutingRules
    {
        public static IfMatches IfTrue(Func<IBotContext, Task<MatcherResult>> conditionAsync)
        {
            return new IfTrue(conditionAsync);
        }

        public static Router IfTrue(Func<IBotContext, Task<MatcherResult>> conditionAsync, Router thenTry)
        {
            return new IfTrue(conditionAsync).ThenTry(thenTry);
        }

        //public static Router IfTrue(Func<IBotContext, MatcherResult> condition, Router thenDo)
        //{
        //    if (condition == null)
        //        throw new ArgumentNullException(nameof(condition));

        //    return IfTrue((context) => Task.FromResult(condition(context)), thenDo);
        //}

        //public static Router Do(Func<Task> handler)
        //{
        //    return new Router((context, routePath) => Task.FromResult(new Route(handler)));
        //}

        public static Router DoNothing(string reason = "none")
        {
            return new Router((context, routePaths) => Task.FromResult((Route)null));
        }
    }
}
