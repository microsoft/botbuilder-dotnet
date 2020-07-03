using System;
using System.Collections.Generic;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Multiply : MultivariateNumericEvaluator
    {
        public Multiply(string alias = null)
            : base(alias ?? ExpressionType.Multiply, Evaluator)
        {
        }

        private static object Evaluator(IReadOnlyList<object> args)
        {
            return EvalMultiply(args[0], args[1]);
        }

        private static object EvalMultiply(object a, object b)
        {
            if (a == null || b == null)
            {
                throw new ArgumentNullException();
            }

            if (a.IsInteger() && b.IsInteger())
            {
                return Convert.ToInt64(a) * Convert.ToInt64(b);
            }
            else
            {
                return FunctionUtils.CultureInvariantDoubleConvert(a) * FunctionUtils.CultureInvariantDoubleConvert(b);
            }
        }
    }
}
