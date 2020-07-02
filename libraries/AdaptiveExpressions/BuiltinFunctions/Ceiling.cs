using System;
using System.Collections.Generic;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Ceiling : NumberTransformEvaluator
    {
        public Ceiling(string alias = null)
                : base(ExpressionType.Ceiling, Function)
        {
        }

        private static object Function(IReadOnlyList<object> args)
        {
            return Math.Ceiling(Convert.ToDouble(args[0]));
        }
    }
}
