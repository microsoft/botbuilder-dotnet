using System.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class SortByDescending : ExpressionEvaluator
    {
        public SortByDescending(string alias = null)
            : base(alias ?? ExpressionType.SortByDescending, FunctionUtils.SortBy(true), ReturnType.Array, Validator)
        {
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, new[] { ReturnType.String }, ReturnType.Array);
        }
    }
}
