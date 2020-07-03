using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Month : ExpressionEvaluator
    {
        public Month(string alias = null)
            : base(alias ?? ExpressionType.Month, Evaluator(), ReturnType.Number, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithError(args => FunctionUtils.NormalizeToDateTime(args[0], dt => dt.Month));
        }
    }
}
