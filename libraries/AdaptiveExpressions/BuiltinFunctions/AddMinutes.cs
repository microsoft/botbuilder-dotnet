using System;
using System.Collections.Generic;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class AddMinutes : TimeTransformEvaluator
    {
        public AddMinutes(string alias = null)
                : base(alias ?? ExpressionType.AddMinutes, Function)
        {
        }

        private static DateTime Function(DateTime time, int interval)
        {
            return time.AddMinutes(interval);
        }
    }
}
