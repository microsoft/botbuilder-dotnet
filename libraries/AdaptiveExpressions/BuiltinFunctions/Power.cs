using System;
using System.Collections;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Power : ExpressionEvaluator
    {
        public Power()
            : base(ExpressionType.Power, Evaluator(), ReturnType.Number, FunctionUtils.ValidateAtLeastOne)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(
                args => Math.Pow(FunctionUtils.CultureInvariantDoubleConvert(args[0]), FunctionUtils.CultureInvariantDoubleConvert(args[1])),
                FunctionUtils.VerifyNumericListOrNumber);
        }
    }
}
