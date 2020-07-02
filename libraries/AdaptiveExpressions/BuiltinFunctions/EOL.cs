using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class EOL : ExpressionEvaluator
    {
        public EOL()
            : base(ExpressionType.EOL, Evaluator(), ReturnType.String, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => Environment.NewLine);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateArityAndAnyType(expression, 0, 0);
        }
    }
}
