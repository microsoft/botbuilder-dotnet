using System;
using Newtonsoft.Json;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class String : ExpressionEvaluator
    {
        public String()
            : base(ExpressionType.String, Evaluator(), ReturnType.String, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => JsonConvert.SerializeObject(args[0]).TrimStart('"').TrimEnd('"'));
        }
    }
}
