using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class IsInteger : ExpressionEvaluator
    {
        public IsInteger(string alias = null)
            : base(alias ?? ExpressionType.IsInteger, Evaluator(), ReturnType.Boolean, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => Extensions.IsNumber(args[0]) && FunctionUtils.CultureInvariantDoubleConvert(args[0]) % 1 == 0);
        }
    }
}
