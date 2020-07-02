using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Year : ExpressionEvaluator
    {
        public Year()
            : base(ExpressionType.Year, Evaluator(), ReturnType.Number, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithError(args => FunctionUtils.NormalizeToDateTime(args[0], dt => dt.Year));
        }
    }
}
