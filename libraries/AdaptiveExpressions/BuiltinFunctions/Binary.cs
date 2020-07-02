using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Binary : ExpressionEvaluator
    {
        public Binary(string alias = null))
            : base(ExpressionType.Binary, Evaluator(), ReturnType.Object, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => FunctionUtils.ToBinary(args[0].ToString()));
        }
    }
}
