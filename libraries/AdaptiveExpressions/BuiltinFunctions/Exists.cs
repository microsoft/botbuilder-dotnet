using System.Collections.Generic;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Exists : ComparisonEvaluator
    {
        public Exists(string alias = null)
            : base(
                  alias ?? ExpressionType.Exists,
                  Function,
                  FunctionUtils.ValidateUnary,
                  FunctionUtils.VerifyNotNull)
        {
        }

        private static bool Function(IReadOnlyList<object> args)
        {
            return args[0] != null;
        }
    }
}
