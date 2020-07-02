using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class DayOfYear : ExpressionEvaluator
    {
        public DayOfYear()
            : base(ExpressionType.DayOfYear, Evaluator(), ReturnType.Number, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithError(args => FunctionUtils.NormalizeToDateTime(args[0], dt => dt.DayOfYear));
        }
    }
}
