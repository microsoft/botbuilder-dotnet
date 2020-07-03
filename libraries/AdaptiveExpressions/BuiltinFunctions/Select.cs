using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Select : ExpressionEvaluator
    {
        public Select(string alias = null)
            : base(alias ?? ExpressionType.Select, FunctionUtils.Foreach, ReturnType.Array, FunctionUtils.ValidateForeach)
        {
        }
    }
}
