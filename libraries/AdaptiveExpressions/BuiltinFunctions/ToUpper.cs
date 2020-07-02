using System;
using System.Collections.Generic;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class ToUpper : StringTransformEvaluator
    {
        public ToUpper()
            : base(ExpressionType.ToUpper, Function)
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
                return args[0].ToString().ToUpperInvariant();
            }
        }
    }
}
