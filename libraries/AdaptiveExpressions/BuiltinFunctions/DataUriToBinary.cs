using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class DataUriToBinary : ExpressionEvaluator
    {
        public DataUriToBinary(string alias = null)
            : base(alias ?? ExpressionType.DataUriToBinary, Evaluator(), ReturnType.Object, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => FunctionUtils.ToBinary(args[0].ToString()), FunctionUtils.VerifyString);
        }
    }
}
