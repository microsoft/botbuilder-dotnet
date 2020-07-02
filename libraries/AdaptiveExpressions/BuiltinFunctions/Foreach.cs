using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Foreach : ExpressionEvaluator
    {
        public Foreach()
            : base(ExpressionType.Foreach, FunctionUtils.Foreach, ReturnType.Array, FunctionUtils.ValidateForeach)
        {
        }
    }
}
