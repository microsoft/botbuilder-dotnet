using System;
using System.Collections.Generic;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class AddSeconds : TimeTransformEvaluator
    {
        public AddSeconds(string alias = null)
                : base(alias ?? ExpressionType.AddSeconds, Function)
        {
        }

        private static DateTime Function(DateTime time, int interval)
        {
            return time.AddSeconds(interval);
        }
    }
}
