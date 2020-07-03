using System.Collections.Generic;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class LessThanOrEqual : ComparisonEvaluator
    {
        public LessThanOrEqual(string alias = null)
            : base(
                  alias ?? ExpressionType.LessThanOrEqual,
                  Function,
                  FunctionUtils.ValidateBinaryNumberOrString,
                  FunctionUtils.VerifyNumberOrString)
        {
        }

        private static bool Function(IReadOnlyList<object> args)
        {
            return FunctionUtils.CultureInvariantDoubleConvert(args[0]) <= FunctionUtils.CultureInvariantDoubleConvert(args[1]);
        }
    }
}
