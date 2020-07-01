using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class NewGuid : ExpressionEvaluator
    {
        public NewGuid()
            : base(ExpressionType.NewGuid, Function(), ReturnType.String, Validator())
        {
        }

        private static EvaluateExpressionDelegate Function()
        {
            return Apply(args => Guid.NewGuid().ToString());
        }

        private static ValidateExpressionDelegate Validator()
        {
            return (exprssion) => ValidateArityAndAnyType(exprssion, 0, 0);
        }
    }
}
