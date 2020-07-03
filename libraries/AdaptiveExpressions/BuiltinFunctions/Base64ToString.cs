using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Base64ToString : ExpressionEvaluator
    {
        public Base64ToString(string alias = null)
            : base(alias ?? ExpressionType.Base64ToString, Evaluator(), ReturnType.String, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(args[0].ToString())), FunctionUtils.VerifyString);
        }
    }
}
