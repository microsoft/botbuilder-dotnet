using System.Collections.Generic;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Exists : ComparisonEvaluator
    {
        public Exists()
            : base(
                  ExpressionType.Exists,
                  Function,
                  FunctionUtils.ValidateBinaryNumberOrString,
                  FunctionUtils.VerifyNumberOrString)
        {
        }

        private static bool Function(IReadOnlyList<object> args)
        {
            return args[0] != null;
        }
    }
}
