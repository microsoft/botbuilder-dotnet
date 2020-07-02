using System.Collections.Generic;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Bool : ComparisonEvaluator
    {
        public Bool(string alias = null))
            : base(
                  ExpressionType.Bool,
                  Function,
                  FunctionUtils.ValidateBinaryNumberOrString,
                  FunctionUtils.VerifyNumberOrString)
        {
        }

        private static bool Function(IReadOnlyList<object> args)
        {
            return FunctionUtils.IsLogicTrue(args[0]);
        }
    }
}
