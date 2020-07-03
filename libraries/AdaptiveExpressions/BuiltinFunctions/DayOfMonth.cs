using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class DayOfMonth : ExpressionEvaluator
    {
        public DayOfMonth(string alias = null)
            : base(alias ?? ExpressionType.DayOfMonth, Evaluator(), ReturnType.Number, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithError(args => FunctionUtils.NormalizeToDateTime(args[0], dt => dt.Day));
        }
    }
}
