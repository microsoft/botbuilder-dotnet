using System;
using System.Collections.Generic;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class ToLower : StringTransformEvaluator
    {
        public ToLower(string alias = null)
            : base(alias ?? ExpressionType.ToLower, Function)
        {
        }

        private static object Function(IReadOnlyList<object> args)
        {
            if (args[0] == null)
            {
                return string.Empty;
            }
            else
            {
                return args[0].ToString().ToLowerInvariant();
            }
        }
    }
}
