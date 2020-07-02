using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Select : ExpressionEvaluator
    {
        public Select()
            : base(ExpressionType.Select, FunctionUtils.Foreach, ReturnType.Array, FunctionUtils.ValidateForeach)
        {
        }
    }
}
