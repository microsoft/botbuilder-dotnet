using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class UriComponent : ExpressionEvaluator
    {
        public UriComponent()
            : base(ExpressionType.UriComponent, Evaluator(), ReturnType.Object, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => Uri.EscapeDataString(args[0].ToString()), FunctionUtils.VerifyString);
        }
    }
}
