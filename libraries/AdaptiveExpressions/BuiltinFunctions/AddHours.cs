using System;
using System.Collections.Generic;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class AddHours : TimeTransformEvaluator
    {
        public AddHours(string alias = null)
                : base(alias ?? ExpressionType.AddHours, Function)
        {
        }

        private static DateTime Function(DateTime time, int interval)
        {
            return time.AddHours(interval);
        }
    }
}
