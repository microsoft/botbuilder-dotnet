using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Float : ExpressionEvaluator
    {
        public Float(string alias = null)
            : base(alias ?? ExpressionType.Float, Evaluator(), ReturnType.Number, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => FunctionUtils.CultureInvariantDoubleConvert(args[0]));
        }
    }
}
