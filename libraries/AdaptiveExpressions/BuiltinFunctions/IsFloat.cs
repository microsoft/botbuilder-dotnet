using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class IsFloat : ExpressionEvaluator
    {
        public IsFloat(string alias = null)
            : base(alias ?? ExpressionType.IsFloat, Evaluator(), ReturnType.Boolean, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => Extensions.IsNumber(args[0]) && FunctionUtils.CultureInvariantDoubleConvert(args[0]) % 1 != 0);
        }
    }
}
