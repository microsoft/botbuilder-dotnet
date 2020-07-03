using System;
using System.Collections.Generic;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class AddDays : TimeTransformEvaluator
    {
        public AddDays(string alias = null)
                : base(alias ?? ExpressionType.AddHours, Function)
        {
        }

        private static DateTime Function(DateTime time, int interval)
        {
            return time.AddDays(interval);
        }
    }
}
