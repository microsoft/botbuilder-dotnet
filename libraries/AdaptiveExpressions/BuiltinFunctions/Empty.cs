using System.Collections.Generic;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Empty : ComparisonEvaluator
    {
        public Empty()
            : base(
                  ExpressionType.Empty,
                  Function,
                  FunctionUtils.ValidateBinaryNumberOrString,
                  FunctionUtils.VerifyNumberOrString)
        {
        }

        private static bool Function(IReadOnlyList<object> args)
        {
            return IsEmpty(args[0]);
        }

        private static bool IsEmpty(object instance)
        {
            bool result;
            if (instance == null)
            {
                result = true;
            }
            else if (instance is string string0)
            {
                result = string.IsNullOrEmpty(string0);
            }
            else if (FunctionUtils.TryParseList(instance, out var list))
            {
                result = list.Count == 0;
            }
            else
            {
                result = instance.GetType().GetProperties().Length == 0;
            }

            return result;
        }
    }
}
