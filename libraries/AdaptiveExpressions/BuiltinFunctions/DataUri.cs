using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class DataUri : ExpressionEvaluator
    {
        public DataUri()
            : base(ExpressionType.DataUri, Evaluator(), ReturnType.String, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => "data:text/plain;charset=utf-8;base64," + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(args[0].ToString())), VerifyString);
        }
    }
}
