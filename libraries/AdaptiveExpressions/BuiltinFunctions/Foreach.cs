using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Foreach : ExpressionEvaluator
    {
        public Foreach(string alias = null)
            : base(alias ?? ExpressionType.Foreach, FunctionUtils.Foreach, ReturnType.Array, FunctionUtils.ValidateForeach)
        {
        }
    }
}
