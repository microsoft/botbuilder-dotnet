using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Divide : ExpressionEvaluator
    {
        public Divide()
            : base(ExpressionType.Divide, Evaluator(), ReturnType.Number, FunctionUtils.ValidateTwoOrMoreThanTwoNumbers)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplySequence(
                args => EvalDivide(args[0], args[1]),
                (val, expression, pos) =>
                {
                    var error = FunctionUtils.VerifyNumber(val, expression, pos);
                    if (error == null && (pos > 0 && Convert.ToSingle(val) == 0.0))
                    {
                        error = $"Cannot divide by 0 from {expression}";
                    }

                    return error;
                });
        }

        private static object EvalDivide(object a, object b)
        {
            if (a == null || b == null)
            {
                throw new ArgumentNullException();
            }

            if (a.IsInteger() && b.IsInteger())
            {
                return Convert.ToInt64(a) / Convert.ToInt64(b);
            }
            else
            {
                return FunctionUtils.CultureInvariantDoubleConvert(a) / FunctionUtils.CultureInvariantDoubleConvert(b);
            }
        }
    }
}
