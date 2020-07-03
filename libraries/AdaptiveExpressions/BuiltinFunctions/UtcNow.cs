using System;
using System.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class UtcNow : ExpressionEvaluator
    {
        public UtcNow(string alias = null)
            : base(alias ?? ExpressionType.UtcNow, Evaluator(), ReturnType.String, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => DateTime.UtcNow.ToString(args.Count() == 1 ? args[0].ToString() : FunctionUtils.DefaultDateTimeFormat));
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, new[] { ReturnType.String });
        }
    }
}
