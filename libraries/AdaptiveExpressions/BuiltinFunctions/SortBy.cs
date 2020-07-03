using System.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class SortBy : ExpressionEvaluator
    {
        public SortBy(string alias = null)
            : base(alias ?? ExpressionType.SortBy, FunctionUtils.SortBy(false), ReturnType.Array, Validator)
        {
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, new[] { ReturnType.String }, ReturnType.Array);
        }
    }
}
