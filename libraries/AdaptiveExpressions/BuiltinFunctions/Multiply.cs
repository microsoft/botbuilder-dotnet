using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Multiply : ExpressionEvaluator
    {
        public Multiply()
            : base(ExpressionType.Multiply, Evaluator(), ReturnType.Number, FunctionUtils.ValidateTwoOrMoreThanTwoNumbers)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplySequence(args => EvalMultiply(args[0], args[1]), FunctionUtils.VerifyNumber);
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
