using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class UriComponent : ExpressionEvaluator
    {
        public UriComponent(string alias = null)
            : base(alias ?? ExpressionType.UriComponent, Evaluator(), ReturnType.Object, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => Uri.EscapeDataString(args[0].ToString()), FunctionUtils.VerifyString);
        }
    }
}
